using DAL.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using RestSharp;
using Newtonsoft.Json.Linq;
using DAL.Security;

namespace DAL.ORM
{
    public class KironRepository
    {
        private readonly string _connectionString;
        private readonly IMemoryCache _cache;
        private static readonly object _regionLock = new object();
        private static readonly object _holidayLock = new object();
        private static readonly object _singleHolidayLock = new object();
        private static readonly object _singleRegionLock = new object();
        private readonly string _coinStatsCacheKey = "CoinStats";
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1);

        public KironRepository(IConfiguration configuration, IMemoryCache cache)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _cache = cache;
        }

        // Method to return all regions with caching and thread safety
        public async Task<List<Region>> GetAllRegionsAsync()
        {
            if (!_cache.TryGetValue("AllRegions", out List<Region> regions))
            {
                lock (_regionLock)
                {
                    if (!_cache.TryGetValue("AllRegions", out regions))
                    {
                        regions = new List<Region>();

                        using var connectionManager = new DbConnectionManager(_connectionString);
                        using var connection = connectionManager.GetConnection();
                        using var command = new SqlCommand("GetAllRegions", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            regions.Add(new Region
                            {
                                RegionId = reader.GetInt32(reader.GetOrdinal("RegionId")),
                                RegionName = reader.GetString(reader.GetOrdinal("RegionName"))
                            });
                        }

                        // Set cache for 30 minutes
                        _cache.Set("AllRegions", regions, TimeSpan.FromMinutes(30));
                    }
                }
            }

            return regions;
        }

        // Method to return all bank holidays for a specific region with caching and thread safety
        public async Task<List<Holiday>> GetHolidaysByRegionAsync(int regionId)
        {
            var cacheKey = $"RegionHolidays_{regionId}";
            if (!_cache.TryGetValue(cacheKey, out List<Holiday> holidays))
            {
                lock (_holidayLock)
                {
                    if (!_cache.TryGetValue(cacheKey, out holidays))
                    {
                        holidays = new List<Holiday>();

                        using var connectionManager = new DbConnectionManager(_connectionString);
                        using var connection = connectionManager.GetConnection();
                        using var command = new SqlCommand("GetHolidaysByRegion", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@RegionId", regionId);

                        using var reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            holidays.Add(new Holiday
                            {
                                HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                HolidayDate = reader.GetFieldValue<DateOnly>(reader.GetOrdinal("HolidayDate")),
                                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                                Bunting = reader.IsDBNull(reader.GetOrdinal("Bunting")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("Bunting"))
                            });
                        }

                        // Set cache for 30 minutes
                        _cache.Set(cacheKey, holidays, TimeSpan.FromMinutes(30));
                    }
                }
            }

            return holidays;
        }

        // Insert Holiday with Regions using stored procedure
        public async Task<int> InsertHolidayWithRegionsAsync(Holiday holiday, List<int> regionIds)
        {
            using var connectionManager = new DbConnectionManager(_connectionString);
            using var connection = connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                // Insert holiday and get the HolidayId
                using var insertHolidayCommand = new SqlCommand("InsertHoliday", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };

                insertHolidayCommand.Parameters.AddWithValue("@Title", holiday.Title);
                insertHolidayCommand.Parameters.AddWithValue("@HolidayDate", holiday.HolidayDate);
                insertHolidayCommand.Parameters.AddWithValue("@Notes", holiday.Notes ?? (object)DBNull.Value);
                insertHolidayCommand.Parameters.AddWithValue("@Bunting", holiday.Bunting ?? (object)DBNull.Value);

                var holidayIdResult = await insertHolidayCommand.ExecuteScalarAsync();
                int holidayId = Convert.ToInt32(holidayIdResult);

                // Insert Region-Holidays associations
                foreach (var regionId in regionIds)
                {
                    using var insertRegionHolidayCommand = new SqlCommand("InsertRegionHoliday", connection, transaction)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    insertRegionHolidayCommand.Parameters.AddWithValue("@HolidayId", holidayId);
                    insertRegionHolidayCommand.Parameters.AddWithValue("@RegionId", regionId);

                    await insertRegionHolidayCommand.ExecuteNonQueryAsync();
                }

                transaction.Commit();

                // Invalidate the cache after insertion
                _cache.Remove("AllRegions");
                foreach (var regionId in regionIds)
                {
                    _cache.Remove($"RegionHolidays_{regionId}");
                }

                return holidayId; // Returns the newly inserted HolidayId
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                throw;
            }
        }


        // Update Holiday with Regions using stored procedure
        public async Task UpdateHolidayWithRegionsAsync(Holiday holiday, List<int> regionIds)
        {
            using var connectionManager = new DbConnectionManager(_connectionString);
            using var connection = connectionManager.GetConnection();
            using var transaction = connection.BeginTransaction();

            try
            {
                using var command = new SqlCommand("UpdateHoliday", connection, transaction)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@HolidayId", holiday.HolidayId);
                command.Parameters.AddWithValue("@Title", holiday.Title);
                command.Parameters.AddWithValue("@HolidayDate", holiday.HolidayDate);
                command.Parameters.AddWithValue("@Notes", holiday.Notes ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Bunting", holiday.Bunting ?? (object)DBNull.Value);

                string regionIdsString = string.Join(",", regionIds);
                command.Parameters.AddWithValue("@RegionIds", regionIdsString);

                await command.ExecuteNonQueryAsync();
                transaction.Commit();

                // Invalidate the cache after update
                _cache.Remove("AllRegions");
                foreach (var regionId in regionIds)
                {
                    _cache.Remove($"RegionHolidays_{regionId}");
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        // Insert Region using stored procedure
        public async Task<int> InsertRegionAsync(Region region)
        {
            using var connectionManager = new DbConnectionManager(_connectionString);
            using var connection = connectionManager.GetConnection();

            using var command = new SqlCommand("InsertRegion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@RegionName", region.RegionName);
            var result = await command.ExecuteScalarAsync();

            // Invalidate the region cache after insertion
            _cache.Remove("AllRegions");

            return Convert.ToInt32(result); // Returns the newly inserted RegionId
        }

        // Update Region using stored procedure
        public async Task UpdateRegionAsync(Region region)
        {
            using var connectionManager = new DbConnectionManager(_connectionString);
            using var connection = connectionManager.GetConnection();

            using var command = new SqlCommand("UpdateRegion", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@RegionId", region.RegionId);
            command.Parameters.AddWithValue("@RegionName", region.RegionName);

            await command.ExecuteNonQueryAsync();

            // Invalidate the region cache after update
            _cache.Remove("AllRegions");
        }

        // Get Holiday by Id (deserialization into model object) with caching and thread safety
        public async Task<Holiday> GetHolidayByIdAsync(int holidayId)
        {
            var cacheKey = $"Holiday_{holidayId}";
            if (!_cache.TryGetValue(cacheKey, out Holiday holiday))
            {
                lock (_singleHolidayLock)
                {
                    if (!_cache.TryGetValue(cacheKey, out holiday))
                    {
                        using var connectionManager = new DbConnectionManager(_connectionString);
                        using var connection = connectionManager.GetConnection();

                        using var command = new SqlCommand("GetHolidayById", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@HolidayId", holidayId);

                        using var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            holiday = new Holiday
                            {
                                HolidayId = reader.GetInt32(reader.GetOrdinal("HolidayId")),
                                Title = reader.GetString(reader.GetOrdinal("Title")),
                                HolidayDate = reader.GetFieldValue<DateOnly>(reader.GetOrdinal("HolidayDate")),
                                Notes = reader.IsDBNull(reader.GetOrdinal("Notes")) ? null : reader.GetString(reader.GetOrdinal("Notes")),
                                Bunting = reader.IsDBNull(reader.GetOrdinal("Bunting")) ? (bool?)null : reader.GetBoolean(reader.GetOrdinal("Bunting"))
                            };
                        }

                        // Set cache for 30 minutes
                        _cache.Set(cacheKey, holiday, TimeSpan.FromMinutes(30));
                    }
                }
            }

            return holiday;
        }

        // Get Region by Id (deserialization into model object) with caching and thread safety
        public async Task<Region> GetRegionByIdAsync(int regionId)
        {
            var cacheKey = $"Region_{regionId}";
            if (!_cache.TryGetValue(cacheKey, out Region region))
            {
                lock (_singleRegionLock)
                {
                    if (!_cache.TryGetValue(cacheKey, out region))
                    {
                        using var connectionManager = new DbConnectionManager(_connectionString);
                        using var connection = connectionManager.GetConnection();

                        using var command = new SqlCommand("GetRegionById", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };

                        command.Parameters.AddWithValue("@RegionId", regionId);

                        using var reader = command.ExecuteReader();
                        if (reader.Read())
                        {
                            region = new Region
                            {
                                RegionId = reader.GetInt32(reader.GetOrdinal("RegionId")),
                                RegionName = reader.GetString(reader.GetOrdinal("RegionName"))
                            };
                        }

                        // Set cache for 30 minutes
                        _cache.Set(cacheKey, region, TimeSpan.FromMinutes(30));
                    }
                }
            }

            return region;
        }

        public async Task<List<NavigationItem>> GetNavigationItemsAsync()
        {
            var navigationItems = new List<NavigationItem>();

            using (var connectionManager = new DbConnectionManager(_connectionString))
            using (var connection = connectionManager.GetConnection())
            {
                var query = "SELECT ID, Text, ParentID FROM Navigation";
                using (var command = new SqlCommand(query, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var item = new NavigationItem
                            {
                                Id = reader.GetInt32(0),
                                Text = reader.GetString(1),
                                ParentId = reader.GetInt32(2)
                            };
                            navigationItems.Add(item);
                        }
                    }
                }
            }

            return BuildNavigationTree(navigationItems);
        }

        private List<NavigationItem> BuildNavigationTree(List<NavigationItem> items)
        {
            var lookup = items.ToLookup(item => item.ParentId);
            foreach (var item in items)
            {
                item.Children = lookup[item.Id].ToList();
            }
            return lookup[-1].ToList(); // Return top-level items (ParentID = -1)
        }

        public async Task<List<CoinModel>> GetLatestCoinStatsAsync()
        {
            if (!_cache.TryGetValue(_coinStatsCacheKey, out List<CoinModel> coinStats))
            {
                // Cache miss, make API call
                var options = new RestClientOptions("https://openapiv1.coinstats.app/coins");
                var client = new RestClient(options);
                var request = new RestRequest("");
                request.AddHeader("accept", "application/json");
                request.AddHeader("X-API-KEY", "4UUCmvMXBbiH3kit+vbsT5fo/ORp/8Pa+/bc9ZQanMU=");

                var response = await client.GetAsync(request);

                if (response.IsSuccessful && response.Content != null)
                {
                    // Parse the response JSON
                    var jsonResponse = JObject.Parse(response.Content);
                    var coinsArray = jsonResponse["result"] as JArray;

                    if (coinsArray != null)
                    {
                        // Deserialize the "result" array into a List<CoinModel>
                        coinStats = coinsArray.ToObject<List<CoinModel>>();

                        // Cache the result for 1 hour
                        _cache.Set(_coinStatsCacheKey, coinStats, _cacheDuration);
                    }
                    else
                    {
                        throw new Exception("Coins data not found in the response");
                    }
                }
                else
                {
                    throw new Exception("Failed to retrieve Coin Stats");
                }
            }

            return coinStats;
        }

        public async Task InsertUserAsync(string username, string password)
        {
            var (passwordHash, salt) = PasswordHasher.HashPassword(password);

            using var connectionManager = new DbConnectionManager(_connectionString);
            using var connection = connectionManager.GetConnection();
            using var command = new SqlCommand("InsertUser", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@PasswordHash", passwordHash);
            command.Parameters.AddWithValue("@Salt", salt);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<(int userId, string username, byte[] passwordHash, byte[] salt)> GetUserByUsernameAsync(string username)
        {
            using var connectionManager = new DbConnectionManager(_connectionString);
            using var connection = connectionManager.GetConnection();
            using var command = new SqlCommand("GetUserByUsername", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@Username", username);
            using var reader = await command.ExecuteReaderAsync();
            if (reader.Read())
            {
                return (
                    reader.GetInt32(reader.GetOrdinal("UserId")),
                    reader.GetString(reader.GetOrdinal("Username")),
                    (byte[])reader["PasswordHash"],
                    (byte[])reader["Salt"]
                );
            }

            return (0, null, null, null); // User not found
        }

    }


}

