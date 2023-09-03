using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcService.Protos;
using System;


namespace GrpcService.Services;
public class PersonGrpcService : PersonService.PersonServiceBase
{
    private List<PresonReply> _peoples = new List<PresonReply>
    {
        new PresonReply
        {
            Id = 1,
            FirstName = "masoud",
            LastName = "maleki",
        },
        new PresonReply
        {
            Id = 2,
            FirstName = "ali",
            LastName = "karimi",
        },
        new PresonReply
        {
            Id = 3,
            FirstName = "zahra",
            LastName = "hassani",
        }
    };

    public override async Task CreatePerson(IAsyncStreamReader<CreatePersonRequest> requestStream, IServerStreamWriter<PresonReply> responseStream, ServerCallContext context)
    {
        var id = _peoples.Count;
        await foreach (var person in requestStream.ReadAllAsync())
        {
            PresonReply personReply = new PresonReply
            {
                FirstName = person.FirstName,
                LastName = person.LastName,
                Id = ++id
            };
            _peoples.Add(personReply);
            await responseStream.WriteAsync(personReply);
        }
    }

    public override async Task<Empty> UpdatePerson(UpdatePersonRequest request, ServerCallContext context)
    {
        var personForUpdate = _peoples.FirstOrDefault(x => x.Id == request.Id);

        if (personForUpdate == null) return new Empty();

        personForUpdate.FirstName = request.FirstName;
        personForUpdate.LastName = request.LastName;

        return new Empty();
    }

    public override async Task<Empty> DeletePerson(IAsyncStreamReader<PersonByIdRequest> requestStream, ServerCallContext context)
    {
        await foreach (var person in requestStream.ReadAllAsync())
        {
            var personForDelete = _peoples.FirstOrDefault(x => x.Id == person.Id);
            if (personForDelete != null)
            {
                _peoples.Remove(personForDelete);
            }
        }
        return new Empty();
    }

    public override async Task GetAll(Empty request, IServerStreamWriter<PresonReply> responseStream, ServerCallContext context)
    {
        foreach (var item in _peoples)
        {
            await responseStream.WriteAsync(item);
        }
    }

    public override async Task<PresonReply> GetPersonById(PersonByIdRequest request, ServerCallContext context)
    {
        var person = _peoples.FirstOrDefault(x => x.Id == request.Id);
      
        if (person != null)
            return person;

        throw new RpcException(new Status(StatusCode.NotFound, $"person with id {request.Id} not found"), "not found from server");

    }
}
