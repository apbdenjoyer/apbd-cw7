using apbd_test_test.Models.DTOs;

namespace apbd_test_test.Repositories;

public interface IAnimalsRepository
{
    Task<bool> DoesAnimalExist(int id);
    Task<bool> DoesOwnerExist(int id);
    Task<bool> DoesProcedureExist(int id);
    Task<AnimalDTO> GetAnimal(int id);
    
    // Version with implicit transaction
    Task AddNewAnimalWithProcedures(NewAnimalWithProcedures newAnimalWithProcedures);
    
    // Version with transaction scope
    Task<int> AddAnimal(NewAnimalDTO animal);
    Task AddProcedureAnimal(int animalId, ProcedureWithDate procedure);
}