using api_elise.Data;
using api_elise.Interfaces;
using api_elise.Models;
using Microsoft.EntityFrameworkCore;


namespace api_elise.Repository
{
    public class ModelRepository : IModelRepository
    {

        private readonly ApplicationDbContext _context;
        public ModelRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Return models list
        public ICollection<Model> GetModels()
        {
            return _context.Models.Include(m => m.QRCodes).OrderBy(m => m.Id).ToList();
        }

        // Return model by his Id
        public Model GetModel(int id)
        {
            return _context.Models.Where(m => m.Id == id).Include(m => m.QRCodes).FirstOrDefault();

        }

        // Sometimes when we don't have the Id of the model, we can search the Model by his title
        public Model GetModel(string title)
        {
            var model = _context.Models
                                .Include(m => m.QRCodes)
                                .FirstOrDefault(m => m.Title == title);
            return model;
        }

        public bool Modelexists(int id)
        {
            return _context.Models.Any(m => m.Id == id);
        }


        public bool CreateModel(Model model)
        {
            _context.Models.Add(model);

            foreach (var qrCode in model.QRCodes)
            {
                _context.QRCodes.Add(qrCode);
            }

            return Save();
        }

        
        public bool UpdateModel(int id, Model model)
        {
            _context.Models.Update(model);
            return Save();
        }
        
        public bool DeleteModel(int id)
        {
            var model = GetModel(id);

            _context.Models.Remove(model); // Remove the model
            return Save(); 
        }
       
        public bool Save()
        {
            var saved = _context.SaveChanges(); // Save changes to the context and get the number of state entries written to the database
            return saved > 0 ? true : false; // Return true if one or more state entries were written to the database, otherwise return false

        }
    }
}
