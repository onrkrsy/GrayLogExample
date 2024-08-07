using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace GrayLogExample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        { 
            _logger.LogInformation("Weather forecast requested");
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
            _logger.LogInformation("Returning {Count} weather forecasts", forecast.Length);

            var user = "Onur";
            var timeStamp = DateTime.UtcNow;
            _logger.LogWarning("WARNING message for monitoring on graylog test integration {TimeStamp}  {User}   ", timeStamp, user);
            _logger.LogInformation("INFO message for monitoring on graylog test integration {TimeStamp}  {User}   ", timeStamp, user);
            _logger.LogCritical("Critical message for monitoring on graylog test integration {TimeStamp}  {User}   ", timeStamp, user);
            _logger.LogError("Error message for monitoring on graylog test integration {TimeStamp}  {User}   ", timeStamp, user);


            return forecast;
        }
    }
}
