﻿using Contact.Service.Models;

namespace Contact.Service.Dtos
{
    public class CommunicationUpdateDto
    {
        public string Id { get; set; }

        public CommunicationType CommunicationType { get; set; }

        public string Address { get; set; }

        public string ContactId { get; set; }
    }
}
