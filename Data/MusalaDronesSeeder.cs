using MusalaDrones.Model;

namespace MusalaDrones.Data
{
    public class MusalaDronesSeeder
    {
        private readonly MusalaDronesDbContext _context;

        public MusalaDronesSeeder(MusalaDronesDbContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            // Drones
            if (!_context.Drones.Any())
            {
                var drones = new List<Drone>() 
                {
                    new Drone { ID = new Guid("016b9160-b3e4-41fa-a719-08db2750d620"),
                                SerialNumber = "SN1", Model = EDroneModel.Lightweight, WeightLimit = 500, BatteryCapacity = 100, State = EDroneState.IDLE },
                    new Drone { ID = new Guid("3a5fc575-8620-48e9-d49f-08db275437cf"),
                                SerialNumber = "SN2", Model = EDroneModel.Middleweight, WeightLimit = 300, BatteryCapacity = 60, State = EDroneState.IDLE },
                    new Drone { ID = new Guid("501af72b-3276-44f9-d4a0-08db275437cf"),
                                SerialNumber = "SN3", Model = EDroneModel.Cruiserweight, WeightLimit = 200, BatteryCapacity = 30, State = EDroneState.IDLE },
                    new Drone { ID = new Guid("4573e3f1-0e4c-4527-d4a1-08db275437cf"),
                                SerialNumber = "SN4", Model = EDroneModel.Heavyweight, WeightLimit = 400, BatteryCapacity = 15, State = EDroneState.IDLE }
                };
                _context.Drones.AddRange(drones);
            }

            // Medication
            if (!_context.Medications.Any())
            {
                var medications = new List<Medication>()
                {
                    new Medication { ID = new Guid("016b9160-b3e4-41fa-a719-08db2750d620"),
                                     Code = "MED01", Name = "Ativan", Weight = 10 },
                    new Medication { ID = new Guid("3a5fc575-8620-48e9-d49f-08db275437cf"),
                                     Code = "MED02", Name = "Hydroxyzine Pamoate", Weight = 30 },
                    new Medication { ID = new Guid("016b9161-b3e4-41fa-a719-08db2750d623"),
                                     Code = "MED03", Name = "Glucosamine", Weight = 20 },
                    new Medication { ID = new Guid("501af72b-3276-44f9-d4a0-08db275437cf"),
                                     Code = "MED04", Name = "Clotrimazole", Weight = 50 },
                    new Medication { ID = new Guid("4573e3f1-0e4c-4527-d4a1-08db275437cf"),
                                     Code = "MED05", Name = "Ubrogepant", Weight = 100 }
                };
                _context.Medications.AddRange(medications);
            }

            _context.SaveChanges();
        }
    }
}
