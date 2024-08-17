using api_elise.Models;

namespace api_elise.Interfaces
{
    public interface IModelRepository
    {
        ICollection<Model> GetModels();
        Model GetModel(int id);
        Model GetModel(string title);
        bool Modelexists(int id);
        bool CreateModel(Model model);
        bool UpdateModel(int id, Model model);
        bool DeleteModel(int id);
        bool Save();
    }
}
