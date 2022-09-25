﻿using AutoMapper;
using Contact.Service.Dtos;
using Contact.Service.Models;
using Contact.Service.Settings;
using MongoDB.Driver;
using Shared.Dtos;
using ContactModel = Contact.Service.Models.Contact;

namespace Contact.Service.Services
{
    public class ContactService : IContactService
    {
        #region private
        private readonly IMongoCollection<ContactModel> _contactCollection;
        private readonly ICommunicationService _communicationService;
        private readonly IMapper _mapper;
        #endregion

        #region ctor
        public ContactService(IMapper mapper, IDatabaseSettings databaseSettings)
        {
            var client = new MongoClient(databaseSettings.ConnectionString);
            var database = client.GetDatabase(databaseSettings.DatabaseName);
            _contactCollection = database.GetCollection<ContactModel>(databaseSettings.ContactCollectionName);

            _mapper = mapper;
        }
        #endregion

        #region methods
        public async Task<Response<List<ContactDto>>> GetAllAsync()
        {
            var contact = await _contactCollection.Find(x => true).ToListAsync();
            return Response<List<ContactDto>>.Success(_mapper.Map<List<ContactDto>>(contact), 200);
        }

        public async Task<Response<ContactWithCommunicationsDto>> GetById(string id)
        {
            var contact = await _contactCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
            if (contact == null)
            {
                return Response<ContactWithCommunicationsDto>.Fail("Contact not found!", 404);
            }
            var contactDto = _mapper.Map<ContactWithCommunicationsDto>(contact);
            contactDto.Communications = _communicationService.GetAllByContactId(contact.Id).Result.Data;

            return Response<ContactWithCommunicationsDto>.Success(contactDto, 200);
        }
        public async Task<Response<ContactDto>> CreateAsync(ContactCreateDto contact)
        {
            var newContact = _mapper.Map<ContactModel>(contact);
            await _contactCollection.InsertOneAsync(newContact);
            return Response<ContactDto>.Success(_mapper.Map<ContactDto>(newContact), 200);
        }
        public async Task<Response<NoContent>> UpdateAsync(ContactUpdateDto contact)
        {
            var updateContact = _mapper.Map<ContactModel>(contact);
            var result = await _contactCollection.FindOneAndReplaceAsync(x => x.Id == updateContact.Id, updateContact);
            if (result == null)
            {
                return Response<NoContent>.Fail("Contact not found!", 404);
            }
            return Response<NoContent>.Success(204);
        }
        public async Task<Response<NoContent>> DeleteAsync(string id)
        {
            var result = await _contactCollection.DeleteOneAsync(x => x.Id == id);
            if (result.DeletedCount > 0)
            {
                return Response<NoContent>.Fail("Contact not found!", 404);
            }
            return Response<NoContent>.Success(204);
        }
        #endregion
    }
}
