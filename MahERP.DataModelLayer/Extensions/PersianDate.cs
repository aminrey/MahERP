using System;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using MahERP.CommonLayer.PublicClasses;
using Microsoft.Extensions.Caching.Memory;

public class PersianDateHelper
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;

    public PersianDateHelper(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    public async Task<string> GetNextWorkingDayDeliveryAsync()
    {
        PersianCalendar persianCalendar = new PersianCalendar();
        DateTime today = DateTime.Now;
        DateTime nextDay = today.AddDays(1);

        while (!await IsWorkingDayAsync(nextDay))
        {
            nextDay = nextDay.AddDays(1);
        }

        string persianDate = ConvertDateTime.ConvertMiladiToShamsi(nextDay, "dddd، d MMMM yyyy");
        return persianDate;
    }
    private async Task<bool> IsWorkingDayAsync(DateTime date)
    {
        PersianCalendar persianCalendar = new PersianCalendar();
        int year = persianCalendar.GetYear(date);
        int month = persianCalendar.GetMonth(date);
        int day = persianCalendar.GetDayOfMonth(date);

        // Check if it's Friday
        if (date.DayOfWeek == DayOfWeek.Friday)
            return false;

        // Cache key for this specific date
        string cacheKey = $"Holiday_{year}_{month}_{day}";

        // Try to get from cache
        if (_memoryCache.TryGetValue(cacheKey, out bool isWorkingDay))
        {
            return isWorkingDay;
        }

        // Calculate the absolute expiration time (midnight tonight or next day)
        DateTime now = DateTime.Now;
        DateTime midnightTonight = now.Date.AddDays(1); // Midnight of the next day (00:00)

        // Call the API if not in cache or cache is expired
        string url = $"https://holidayapi.ir/jalali/{year}/{month}/{day}";
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            string jsonResponse = await httpClient.GetStringAsync(url);

            // Use case-insensitive deserialization
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
            var response = JsonSerializer.Deserialize<ShamsiHolidayApiResponse>(jsonResponse, options);

            // Log the response for debugging
            Console.WriteLine($"API Response for {year}/{month}/{day}: {jsonResponse}");

            // Check if response is null
            if (response == null)
            {
                throw new Exception("Failed to deserialize API response.");
            }

            // Determine if it's a working day
            bool isHoliday = response.is_holiday || (response.Events?.Any(e => e.is_holiday) ?? false);
            bool isWorking = !isHoliday;

            // Cache the result until midnight
            var cacheEntryOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = midnightTonight
            };
            _memoryCache.Set(cacheKey, isWorking, cacheEntryOptions);

            return isWorking;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calling API or deserializing: {ex.Message}");
            return true; // Fallback
        }
    }


    public class ShamsiHolidayApiResponse
    {
        public bool is_holiday { get; set; }
        public List<Event> Events { get; set; }
    }

    public class Event
    {
        public string? description { get; set; }
        public string? additional_description { get; set; }
        public bool is_religious { get; set; }
        public bool is_holiday { get; set; }
    }
}
