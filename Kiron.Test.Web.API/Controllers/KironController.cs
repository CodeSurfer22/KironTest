using DAL.Models;
using DAL.ORM;
using DAL.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Kiron.Test.Web.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KironController : ControllerBase
    {
        private readonly KironRepository _repository;
        private readonly IConfiguration _configuration;
        private static bool _workCompleted = false;

        public KironController(KironRepository repository, IConfiguration configuration)
        {
            _repository = repository;
            _configuration = configuration;
        }

        // Endpoint to get hierarchical navigation data
        [HttpGet("navigation")]
        public async Task<IActionResult> GetNavigationTree()
        {
            var navigationTree = await _repository.GetNavigationItemsAsync();
            return Ok(navigationTree);
        }

        // Endpoint to get the latest Coin Stats
        //Expose an endpoint that will make a call to retrieve the latest Coin Stats. https://api.coinstats.app/public/v1/coins
        //"The above API is deprecated and will be disabled by Oct 31 2023, to use the new version please go to https://openapi.coinstats.app ."
        //That is exactly what I did.
        [HttpGet("coin-stats")]
        public async Task<IActionResult> GetCoinStats()
        {
            try
            {
                var coinStats = await _repository.GetLatestCoinStatsAsync();
                return Ok(coinStats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        #region Security
        [HttpPost("register")]
        public async Task<IActionResult> Register(string username, string password)
        {
            await _repository.InsertUserAsync(username, password);
            return Ok("User registered successfully.");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string username, string password)
        {
            var (userId, dbUsername, dbPasswordHash, dbSalt) = await _repository.GetUserByUsernameAsync(username);
            if (userId == 0 || !PasswordHasher.VerifyPassword(password, dbPasswordHash, dbSalt))
            {
                return Unauthorized("Invalid username or password.");
            }

            var token = GenerateJwtToken(username);
            return Ok(new { Token = token });
        }

        private string GenerateJwtToken(string username)
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        #endregion


        #region Regions

        // Endpoint to return all regions
        [HttpGet("regions/all")]
        public async Task<IActionResult> GetAllRegions()
        {
            var regions = await _repository.GetAllRegionsAsync();
            return Ok(regions);
        }

        // Endpoint to get a specific region by ID
        [HttpGet("regions/{regionId}")]
        public async Task<IActionResult> GetRegionById(int regionId)
        {
            var region = await _repository.GetRegionByIdAsync(regionId);
            if (region == null)
            {
                return NotFound();
            }
            return Ok(region);
        }

        // Endpoint to insert a new region
        [HttpPost("regions")]
        public async Task<IActionResult> InsertRegion([FromBody] Region region)
        {
            if (region == null)
            {
                return BadRequest();
            }

            var regionId = await _repository.InsertRegionAsync(region);
            return CreatedAtAction(nameof(GetRegionById), new { regionId = regionId }, region);
        }

        // Endpoint to update an existing region
        [HttpPut("regions")]
        public async Task<IActionResult> UpdateRegion([FromBody] Region region)
        {
            if (region == null)
            {
                return BadRequest();
            }

            await _repository.UpdateRegionAsync(region);
            return NoContent();
        }

        #endregion

        #region Holidays

        // Endpoint to return all holidays for a specific region
        [HttpGet("holidays/region/{regionId}")]
        public async Task<IActionResult> GetHolidaysByRegion(int regionId)
        {
            var holidays = await _repository.GetHolidaysByRegionAsync(regionId);
            return Ok(holidays);
        }

        // Endpoint to get a holiday by ID
        [HttpGet("holidays/{holidayId}")]
        public async Task<IActionResult> GetHolidayById(int holidayId)
        {
            var holiday = await _repository.GetHolidayByIdAsync(holidayId);
            if (holiday == null)
            {
                return NotFound();
            }
            return Ok(holiday);
        }

        // Endpoint to insert a new holiday with associated regions
        [HttpPost("holidays")]
        public async Task<IActionResult> InsertHolidayWithRegions([FromBody] HolidayWithRegionsDto dto)
        {
            if (dto == null || dto.Holiday == null || dto.RegionIds == null)
            {
                return BadRequest();
            }

            var holidayId = await _repository.InsertHolidayWithRegionsAsync(dto.Holiday, dto.RegionIds);
            return CreatedAtAction(nameof(GetHolidayById), new { holidayId = holidayId }, dto.Holiday);
        }

        // Endpoint to update an existing holiday with associated regions
        [HttpPut("holidays")]
        public async Task<IActionResult> UpdateHolidayWithRegions([FromBody] HolidayWithRegionsDto dto)
        {
            if (dto == null || dto.Holiday == null || dto.RegionIds == null)
            {
                return BadRequest();
            }

            await _repository.UpdateHolidayWithRegionsAsync(dto.Holiday, dto.RegionIds);
            return NoContent();
        }

        #endregion

        #region UK Bank Holidays

        // New endpoint to fetch and update UK Bank Holidays
        [HttpPost("bank-holidays/fetch")]
        public async Task<IActionResult> FetchAndUpdateBankHolidays()
        {
            if (_workCompleted)
            {
                return BadRequest("Bank holidays data has already been processed.");
            }

            // URL of the external API
            string apiUrl = "https://www.gov.uk/bank-holidays.json";

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Make the request to the external API
                    var response = await httpClient.GetAsync(apiUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        return StatusCode((int)response.StatusCode, "Failed to fetch bank holidays data.");
                    }

                    // Parse the response
                    var content = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(content);

                    // Process each region and holiday
                    foreach (var region in data)
                    {
                        string regionName = region.Key;
                        var events = region.Value["events"];

                        // Insert the region via the existing API endpoint
                        var regionId = await InsertRegionIfNotExists(regionName);

                        // Process holidays in the region
                        foreach (var holidayEvent in events)
                        {
                            var holiday = new Holiday
                            {
                                Title = holidayEvent["title"].ToString(),
                                HolidayDate = DateOnly.FromDateTime(holidayEvent["date"].ToObject<DateTime>()), // Convert DateTime to DateOnly
                                Notes = holidayEvent["notes"].ToString(),
                                Bunting = holidayEvent["bunting"].ToObject<bool>()
                            };


                            // Insert the holiday and associate it with the region
                            await InsertHolidayWithRegion(holiday, regionId);
                        }
                    }

                    // Mark the work as completed
                    _workCompleted = true;
                }

                return Ok("Bank holidays data has been successfully processed.");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(500, $"Error fetching bank holidays: {ex.Message}");
            }
        }

        private async Task<int> InsertRegionIfNotExists(string regionName)
        {
            var regions = await _repository.GetAllRegionsAsync();
            var existingRegion = regions.FirstOrDefault(r => r.RegionName == regionName);

            if (existingRegion == null)
            {
                var region = new Region { RegionName = regionName };
                return await _repository.InsertRegionAsync(region);
            }

            return existingRegion.RegionId;
        }

        private async Task InsertHolidayWithRegion(Holiday holiday, int regionId)
        {
            var regionIds = new List<int> { regionId };

            // Insert holiday and associate with the region using the existing repository method
            await _repository.InsertHolidayWithRegionsAsync(holiday, regionIds);
        }

        #endregion
    }

    // DTO for inserting and updating holidays with regions
    public class HolidayWithRegionsDto
    {
        public Holiday Holiday { get; set; }
        public List<int> RegionIds { get; set; }
    }
}
