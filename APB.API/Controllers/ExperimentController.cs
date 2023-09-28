using ABP.Application.Interfaces;
using ABP.Domain.Entities;
using ABP.Domain.Repository;
using Microsoft.AspNetCore.Mvc;

namespace APB.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ExperimentController : ControllerBase
    {
        private readonly IDeviceRepository _deviceRepository;
        private readonly IExperimentRepository _experimentRepository;
        private readonly IChanceBasedOutputService _chanceBasedOutputService;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _baseUrl;

        private Dictionary<string, double> _buttonColorExperimentValues = new Dictionary<string, double> { { "#FF0000", 0.333 },
                                                                                                          { "#00FF00", 0.333 },
                                                                                                          { "#0000FF", 0.333 }};

        private Dictionary<string, double> _priceExperimentValues = new Dictionary<string, double> { { "10", 0.75 },
                                                                                                     { "20", 0.10 },
                                                                                                     { "50", 0.05 },
                                                                                                     { "5", 0.10 }};
        public ExperimentController(IDeviceRepository deviceRepository,
            IChanceBasedOutputService chanceBasedOutputService,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IExperimentRepository experimentRepository)
        {
            _deviceRepository = deviceRepository;
            _experimentRepository = experimentRepository;
            _chanceBasedOutputService = chanceBasedOutputService;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _baseUrl = _httpContextAccessor.HttpContext.Request.Scheme + "://" +
                            _httpContextAccessor.HttpContext.Request.Host.Value;
        }

        [Route("button_color")]
        [HttpPost]
        public async Task<IActionResult> ButtonColor([FromQuery(Name = "device-token")] string deviceToken)
        {
            string experiment = "button_color";

            var device = await _deviceRepository.GetDeviceAsync(deviceToken);

            if (device != null)
            {
                if (device.FirstRequest < DateTime.Parse(_configuration["Experiment:Created"]))
                    return Redirect("https://www.google.com/");

                if (device.Experiment == experiment)
                {
                    return new JsonResult(new { key = experiment, value = device.ReceivedValue });
                }
                else
                {
                    //Each user can participate only in one experiment
                    return Redirect($"{_baseUrl}/experiment/{device.Experiment}?device-token={deviceToken}");
                }
            }

            string receivedValue = _chanceBasedOutputService.GetRandomValue(_buttonColorExperimentValues);

            await _deviceRepository.AddDeviceAsync(deviceToken, experiment, receivedValue);

            return new JsonResult(new { key = experiment, value = receivedValue });
        }

        [Route("price")]
        [HttpPost]
        public async Task<IActionResult> Price([FromQuery(Name = "device-token")] string deviceToken)
        {
            string experiment = "price";

            var device = await _deviceRepository.GetDeviceAsync(deviceToken);

            if (device != null)
            {
                if (device.FirstRequest < DateTime.Parse(_configuration["Experiment:Created"]))
                    return Redirect("https://www.google.com/");

                if (device.Experiment == experiment)
                {
                    return new JsonResult(new { key = experiment, value = device.ReceivedValue });
                }
                else
                {
                    return Redirect($"{_baseUrl}/experiment/{device.Experiment}?device-token={deviceToken}");
                }
            }

            string receivedValue = _chanceBasedOutputService.GetRandomValue(_priceExperimentValues);

            await _deviceRepository.AddDeviceAsync(deviceToken, experiment, receivedValue);

            return new JsonResult(new { key = experiment, value = receivedValue });
        }

        [Route("statistics")]
        [HttpGet]
        public async Task<IActionResult> Statistics()
        {
            List<Device> devices = await _deviceRepository.GetAllDevicesAsync();

            var experiments = await _experimentRepository.GetAllExperimentsAsync();

            Dictionary<string, Dictionary<string, int>> experimentsSummary =
                new Dictionary<string, Dictionary<string, int>>();

            foreach (var experiment in experiments)
            {
                experimentsSummary[experiment] = new Dictionary<string, int>();

                var experimentVlues = devices.Where(d => d.Experiment == experiment).Select(d => d.ReceivedValue).Distinct();

                foreach (var experimentValue in experimentVlues)
                {
                    experimentsSummary[experiment].Add(experimentValue,
                        devices.Where(d => d.Experiment == experiment && d.ReceivedValue == experimentValue).ToList().Count);
                }
            }

            var result = new
            {
                totalNumberOfDevices = devices.Count,
                experiments = experiments,
                experimentsSummary = experimentsSummary,
                devices = devices
            };

            return new JsonResult(result);
        }
    }
}
