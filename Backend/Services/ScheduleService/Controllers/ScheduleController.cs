using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ScheduleService.Repositories.ScheduleRepository;
using ScheduleService.Dtos;
using ScheduleService.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScheduleService.OtherModels;
using ScheduleService.Service.HttpServices;
using Microsoft.AspNetCore.Authorization;

namespace ScheduleService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleController : ControllerBase
    {
        private readonly IScheduleRepo _repository;
        private readonly IMapper _mapper;
        private readonly MovieHttpService _movieHttpService;
        private readonly TheaterHttpService _theaterHttpService;
        private readonly InvoiceHttpService _invoiceHttpService;
        private readonly ILogger<ScheduleController> _logger;

        public ScheduleController(
            IScheduleRepo repository,
            IMapper mapper,
            MovieHttpService movieHttpService,
            TheaterHttpService theaterHttpService,
            InvoiceHttpService invoiceHttpService,
            ILogger<ScheduleController> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _movieHttpService = movieHttpService;
            _theaterHttpService = theaterHttpService;
            _invoiceHttpService = invoiceHttpService;
            _logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ScheduleReadDto>>> GetAllSchedules()
        {
            var schedules = await _repository.GetAllScheduleAsync();

            var scheduleReadDtos = new List<ScheduleReadDto>();

            foreach (var schedule in schedules)
            {
                var movie = await _movieHttpService.GetMovieById(schedule.MovieId);
                Seat seat = await _theaterHttpService.GetSeatById(schedule.SeatId);
                Room room = (seat != null) ? await _theaterHttpService.GetRoomById(seat.RoomId) : null;
                Theater theater = (room != null) ? await _theaterHttpService.GetTheaterById(room.TheaterId) : null;

                var invoiceId = schedule.InvoiceId.GetValueOrDefault();
                var invoice = (schedule.InvoiceId.HasValue) ? await _invoiceHttpService.GetInvoiceById(invoiceId) : null;

                room.Theater = theater;
                seat.Room = room;

                var scheduleReadDto = new ScheduleReadDto
                {
                    Date = schedule.Date,
                    Time = schedule.Time,
                    InvoiceId = schedule.InvoiceId,
                    Movie = new Movie
                    {
                        Id = movie.Id,
                        Title = movie.Title,
                        MovieUrl = movie.MovieUrl,
                    },
                    Seat = seat,
                    Invoice = invoice,
                };

                scheduleReadDtos.Add(scheduleReadDto);
            }

            return Ok(scheduleReadDtos);
        }

        [HttpGet("GetAllBasicSchedule")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetAllBasicSchedule()
        {
            // Lấy tất cả lịch chiếu từ cơ sở dữ liệu
            var schedules = await _repository.GetAllScheduleAsync();

            // Thu thập các ID duy nhất của Seat, Room, và Movie
            var uniqueSeatIds = schedules.Select(s => s.SeatId).Distinct().ToList();
            var uniqueMovieIds = schedules.Select(s => s.MovieId).Distinct().ToList();
            var uniqueRoomIds = new HashSet<int>();

            var roomDictionary = new Dictionary<int, Room>(); // Key: SeatId, Value: Room
            var theaterDictionary = new Dictionary<int, Theater>(); // Key: RoomId, Value: Theater
            var movieDictionary = new Dictionary<int, Movie>(); // Key: MovieId, Value: Movie

            // Lấy thông tin phòng từ các SeatIds duy nhất
            var roomTasks = uniqueSeatIds.Select(async seatId =>
            {
                var room = await _theaterHttpService.GetRoomBySeatId(seatId);
                if (room != null)
                {
                    roomDictionary[seatId] = room;
                    uniqueRoomIds.Add(room.Id); // Thêm RoomId vào danh sách duy nhất
                }
            });
            await Task.WhenAll(roomTasks);

            // Lấy thông tin rạp chiếu từ các RoomIds duy nhất
            var theaterTasks = uniqueRoomIds.Select(async roomId =>
            {
                var theater = await _theaterHttpService.GetTheaterByRoomId(roomId);
                if (theater != null)
                {
                    theaterDictionary[roomId] = theater;
                }
            });
            await Task.WhenAll(theaterTasks);

            // Lấy thông tin phim từ các MovieIds duy nhất
            var movieTasks = uniqueMovieIds.Select(async movieId =>
            {
                var movie = await _movieHttpService.GetMovieById(movieId);
                if (movie != null)
                {
                    movieDictionary[movieId] = movie;
                }
            });
            await Task.WhenAll(movieTasks);

            // Xây dựng danh sách kết quả từ dữ liệu đã lấy
            var results = schedules
                .GroupBy(s => new { s.SeatId, s.MovieId, s.Date, s.Time })
                .Select(g =>
                {
                    var schedule = g.First(); // Lấy một bản ghi từ nhóm
                    var room = roomDictionary.GetValueOrDefault(schedule.SeatId);
                    var theater = room != null ? theaterDictionary.GetValueOrDefault(room.Id) : null;
                    var movie = movieDictionary.GetValueOrDefault(schedule.MovieId);

                    if (room != null && theater != null && movie != null)
                    {
                        return new
                        {
                            TheaterName = theater.Name,
                            RoomName = room.Name,
                            Date = schedule.Date,
                            MovieName = movie.Title,
                            Time = schedule.Time,
                        };
                    }

                    return null; // Trả về null nếu không đủ thông tin để xây dựng kết quả
                })
                .Where(result => result != null)
                .Distinct() // Loại bỏ các mục trùng lặp
                .ToList();

            return Ok(results);
        }

        [HttpGet("getSeatsBySchedule")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<object>>> GetSeatsBySchedude( int movieId, DateOnly date, int theaterId, int roomId, TimeOnly time)
        {
            var schedules = await _repository.GetSeatsBySchedude(movieId, date, theaterId, roomId, time);
            var room = await _theaterHttpService.GetRoomById(roomId);
            var theater = await _theaterHttpService.GetTheaterById(theaterId);


            var seats = new List<object>();

            foreach (var schedule in schedules)
            {
                var seat = await _theaterHttpService.GetSeatById(schedule.SeatId);
                if (seat.RoomId == roomId)
                {
                    seats.Add(seat);
                }

            }

            object result = new
            {
                Room = room,
                Theater = theater,
                Schedules = schedules,
                Seats = seats,
            };

            return Ok(result);

        }

        [HttpGet("GetOnlyScheduleWithoutSeats")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ScheduleReadDto>>> GetOnlyScheduleWithoutSeats()
        {
            var schedules = await _repository.GetAllScheduleAsync();

            var scheduleReadDtos = new List<ScheduleReadDto>();

            foreach (var schedule in schedules)
            {
                var movie = await _movieHttpService.GetMovieById(schedule.MovieId);

                var scheduleReadDto = new ScheduleReadDto
                {
                    Date = schedule.Date,
                    Time = schedule.Time,
                    InvoiceId = schedule.InvoiceId,
                    Movie = new Movie
                    {
                        Id = movie.Id,
                        Title = movie.Title,
                        MovieUrl = movie.MovieUrl,
                    },
                };

                scheduleReadDtos.Add(scheduleReadDto);
            }

            return Ok(scheduleReadDtos);
        }
        [HttpGet("movieUrl/{url}")]
        [AllowAnonymous]
        public async Task<ActionResult<object>> GetSchedulesByMovieUrl(string url)

        {
            var movie = await _movieHttpService.GetMovieByUrl(url);
            if (movie == null)
            {
                return NotFound();
            }

            var schedules = await _repository.GetSchedulesByMovieIdAsync(movie.Id);
            var scheduleMap = new Dictionary<string, Dictionary<int, TheaterScheduleDto>>();

            foreach (var schedule in schedules)
            {
                var seat = await _theaterHttpService.GetSeatById(schedule.SeatId);
                var room = (seat != null) ? await _theaterHttpService.GetRoomById(seat.RoomId) : null;
                var theater = (room != null) ? await _theaterHttpService.GetTheaterById(room.TheaterId) : null;
                var scheduleDate = schedule.Date.ToString("yyyy-MM-dd");

                if (!scheduleMap.ContainsKey(scheduleDate))
                {
                    scheduleMap[scheduleDate] = new Dictionary<int, TheaterScheduleDto>();
                }

                if (!scheduleMap[scheduleDate].ContainsKey(theater.Id))
                {
                    scheduleMap[scheduleDate][theater.Id] = new TheaterScheduleDto
                    {
                        TheaterId = theater.Id,
                        TheaterName = theater.Name,
                        RoomSchedules = new Dictionary<int, RoomScheduleDto>()
                    };
                }

                var theaterSchedule = scheduleMap[scheduleDate][theater.Id];

                if (!theaterSchedule.RoomSchedules.ContainsKey(room.Id))
                {
                    theaterSchedule.RoomSchedules[room.Id] = new RoomScheduleDto
                    {
                        RoomId = room.Id,
                        RoomName = room.Name,
                        Times = new List<string>()
                    };
                }

                var roomSchedule = theaterSchedule.RoomSchedules[room.Id];

                if (!roomSchedule.Times.Contains(schedule.Time.ToString("HH:mm")))
                {
                    roomSchedule.Times.Add(schedule.Time.ToString("HH:mm"));
                }
            }

            var formattedSchedule = scheduleMap.Select(sm => new
            {
                Date = sm.Key,
                TheaterSchedules = sm.Value.Values.Select(ts => new
                {
                    ts.TheaterId,
                    ts.TheaterName,
                    RoomSchedules = ts.RoomSchedules.Values.Select(rs => new
                    {
                        rs.RoomId,
                        rs.RoomName,
                        Times = rs.Times
                    }).ToList()
                }).ToList()
            }).ToList();

            var result = new
            {
                Movie = new
                {
                    Id = movie.Id,
                    Title = movie.Title,
                    MovieUrl = movie.MovieUrl,
                },
                Schedules = formattedSchedule

            };

            return Ok(result);
        }

        [HttpGet("GetScheduleByInvoiceId/{invoiceId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Schedule>>> GetScheduleByInvoiceId(int invoiceId)
        {
            var schedules = await _repository.GetScheduleByInvoiceIdAsync(invoiceId);

            return Ok(schedules);
        }

        // POST: api/schedule
        [HttpPost]
        [Authorize(Policy ="AdminOnly")]
        public async Task<ActionResult<ScheduleCreateDto>> CreateSchedule(ScheduleCreateDto scheduleCreateDto)
        {
            try
            {
                var schedules = new List<Schedule>();
                foreach (var seatId in scheduleCreateDto.SeatIds)
                {
                    var schedule = new Schedule
                    {
                        MovieId = scheduleCreateDto.MovieId,
                        Date = scheduleCreateDto.Date,
                        Time = scheduleCreateDto.Time,
                        SeatId = seatId,
                        InvoiceId = null
                    };
                    schedules.Add(schedule);
                }

                var createdSchedules = new List<Schedule>();
                foreach (var schedule in schedules)
                {
                    var createdSchedule = await _repository.CreateScheduleAsync(schedule);
                    createdSchedules.Add(createdSchedule);
                }

                var scheduleCreateDtoResponse = new ScheduleCreateDto
                {
                    MovieId = scheduleCreateDto.MovieId,
                    Date = scheduleCreateDto.Date,
                    Time = scheduleCreateDto.Time,
                    SeatIds = createdSchedules.Select(s => s.SeatId).ToList(),
                    InvoiceId = null,
                };

                return CreatedAtAction(nameof(GetAllSchedules), new { id = createdSchedules.First().MovieId }, scheduleCreateDtoResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the schedule.");
                return StatusCode(500, "Internal server error");
            }
        }

    }
    public class TheaterScheduleDto
    {
        public int TheaterId { get; set; } // ID của rạp chiếu phim
        public string TheaterName { get; set; } // Tên của rạp chiếu phim
        public Dictionary<int, RoomScheduleDto> RoomSchedules { get; set; } // Danh sách lịch trình của các phòng chiếu trong rạp
    }

    public class RoomScheduleDto
    {
        public int RoomId { get; set; } // ID của phòng chiếu
        public string RoomName { get; set; } // Tên của phòng chiếu
        public List<string> Times { get; set; } // Danh sách các thời gian chiếu
    }
}
