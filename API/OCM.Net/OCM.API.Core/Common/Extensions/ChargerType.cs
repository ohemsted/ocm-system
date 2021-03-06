﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OCM.API.Common.Model.Extensions
{
    public class ChargerType
    {
        public static Model.ChargerType FromDataModel(Core.Data.ChargerType source)
        {
            if (source == null) return null;

            return new Model.ChargerType { 
                ID = source.ID, 
                Title = source.Title, 
                Comments = source.Comments, 
                IsFastChargeCapable = source.IsFastChargeCapable 
            };
        }
    }
}