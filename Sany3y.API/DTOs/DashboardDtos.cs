using System;
using System.Collections.Generic;

namespace Sany3y.API.DTOs
{
    public class TaskDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime? Date { get; set; } // Assuming date is relevant, maybe from CreatedAt or a scheduled date if it existed
        public string OtherPartyName { get; set; } // Name of the Tasker (for client) or Client (for tasker)
        public decimal Price { get; set; }
        public string ServiceName { get; set; }
        public int? PaymentMethodId { get; set; }
    }

    public class CustomerDashboardDto
    {
        public UserProfileDto Profile { get; set; }
        public List<TaskDto> PastBookings { get; set; }
        public List<TaskDto> UpcomingAppointments { get; set; }
    }

    public class WorkerDashboardDto
    {
        public UserProfileDto Profile { get; set; }
        public List<TaskDto> ReceivedRequests { get; set; }
        public List<TaskDto> AcceptedJobs { get; set; }
        public decimal TotalEarnings { get; set; }
    }

    public class UserProfileDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; } // Simplified address
        public string ProfilePictureUrl { get; set; }
        public string ServiceDetails { get; set; } // For workers, maybe Category or Bio
        public string? ShopName { get; set; }
        public bool? IsShop { get; set; }
    }

    public class UpdateProfileDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Bio { get; set; }
        public string Location { get; set; } // Address string or ID? Let's assume string for now or handle Address update logic
        // Photo update would likely be a separate endpoint or multipart/form-data, keeping it simple here
        public string ServiceDetails { get; set; }
    }
}
