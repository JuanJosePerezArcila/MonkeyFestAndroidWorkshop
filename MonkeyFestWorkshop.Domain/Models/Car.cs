﻿using System.Collections.Generic;

namespace MonkeyFestWorkshop.Domain.Models
{
    public class Car: BaseVehicle
    {
        public string Body { get; set; }

        protected override bool ValidatePlate(string plate)
        {
            throw new System.NotImplementedException();
        }

        protected override IEnumerable<string> ValidateVehicle()
        {
            throw new System.NotImplementedException();
        }
    }
}
