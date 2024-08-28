using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TheaterService.Dtos;
using TheaterService.Models;

namespace TheaterService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeatsController : ControllerBase
    {
        private readonly TheaterContext _context;
        private readonly IMapper _mapper;

        public SeatsController(TheaterContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        // GET: api/Seats
        // Chức năng này có thể cho phép Anonymous truy cập để xem tất cả các ghế
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SeatReadDto>>> GetSeats()
        {
            var seats = await _context.Seats.ToListAsync();
            return Ok(_mapper.Map<IEnumerable<SeatReadDto>>(seats));
        }

        // GET: api/Seats/5
        // Chức năng này có thể cho phép Anonymous truy cập để xem chi tiết một ghế
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<ActionResult<SeatReadDto>> GetSeat(int id)
        {
            var seat = await _context.Seats.FindAsync(id);

            if (seat == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<SeatReadDto>(seat));
        }

        // GET: api/Seats/GetSeatsByRoom/5
        // Chức năng này có thể cho phép Anonymous truy cập để xem các ghế theo RoomId
        [HttpGet("GetSeatsByRoom/{roomId}")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<SeatReadDto>>> GetSeatsByRoom(int roomId)
        {
            var seats = await _context.Seats.Where(s => s.RoomId == roomId).ToListAsync();

            if (seats == null || !seats.Any())
            {
                return NotFound("This room does not have seats, please add more seats.");
            }

            return Ok(_mapper.Map<IEnumerable<SeatReadDto>>(seats));
        }

        // PUT: api/Seats/5
        // Chức năng này chỉ cho phép Admin và Employee có quyền cập nhật thông tin ghế
        [HttpPut("{id}")]
        [Authorize(Policy = "AdminEmployeeOnly")]
        public async Task<IActionResult> PutSeat(int id, SeatCreateDto seatDto)
        {

            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }

            _mapper.Map(seatDto, seat);
            _context.Entry(seat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeatExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpGet("getVenueBySeatId/{seatId}")]
        [Authorize]
        public async Task<ActionResult<object>> GetVenueBySeatId(int seatId)
        {
            // Find the seat by seatId
            var seat = await _context.Seats.FindAsync(seatId);
            if (seat == null)
            {
                return NotFound(new { Message = $"Seat with ID {seatId} not found." });
            }

            // Find the room associated with the seat
            var room = await _context.Rooms.FindAsync(seat.RoomId);
            if (room == null)
            {
                return NotFound(new { Message = $"Room with ID {seat.RoomId} not found." });
            }

            // Find the theater associated with the room
            var theater = await _context.Theaters.FindAsync(room.TheaterId);
            if (theater == null)
            {
                return NotFound(new { Message = $"Theater with ID {room.TheaterId} not found." });
            }

            // Create an anonymous object to return as the result
            var result = new
            {
                RoomId = room.Id,
                RoomName = room.Name,
                TheaterId = theater.Id,
                TheaterName = theater.Name
            };

            // Return the result with an HTTP 200 OK response
            return Ok(result);
        }


        // POST: api/Seats
        // Chức năng này chỉ cho phép Admin thêm ghế mới
        /*[HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<SeatReadDto>> PostSeat(SeatCreateDto seatDto)
        {
            var seat = _mapper.Map<Seat>(seatDto);
            _context.Seats.Add(seat);
            await _context.SaveChangesAsync();

            var seatReadDto = _mapper.Map<SeatReadDto>(seat);

            return CreatedAtAction("GetSeat", new { id = seatReadDto.Id }, seatReadDto);
        }*/

        // POST: api/Seats/AddMultiple
        // Chức năng này chỉ cho phép Admin thêm nhiều ghế cùng một lúc
        [HttpPost]
        [Authorize(Policy = "AdminOnly")]
        public async Task<ActionResult<IEnumerable<SeatReadDto>>> PostMultipleSeats(IEnumerable<SeatCreateDto> seatDtos)
        {
            var seats = _mapper.Map<IEnumerable<Seat>>(seatDtos);
            _context.Seats.AddRange(seats);
            await _context.SaveChangesAsync();

            var seatReadDtos = _mapper.Map<IEnumerable<SeatReadDto>>(seats);

            return Ok(seatReadDtos);
        }

        // DELETE: api/Seats/5
        // Chức năng này chỉ cho phép Admin xóa ghế
        [HttpDelete("{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteSeat(int id)
        {
            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }

            _context.Seats.Remove(seat);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SeatExists(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }
        [HttpPut("{id}/disable")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> HideSeat(int id)
        {
            var seat = await _context.Seats.FindAsync(id);
            if (seat == null)
            {
                return NotFound();
            }

            seat.Status = "Inactive"; // Giả định rằng bạn có một trường Status để ẩn rạp chiếu
            _context.Entry(seat).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SeatExits(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        private bool SeatExits(int id)
        {
            return _context.Seats.Any(e => e.Id == id);
        }

    }
}
