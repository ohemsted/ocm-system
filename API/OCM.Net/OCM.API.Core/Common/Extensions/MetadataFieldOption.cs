﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace OCM.API.Common.Model.Extensions
{
    public class MetadataFieldOption
    {
        public static Model.MetadataFieldOption FromDataModel(Core.Data.MetadataFieldOption source)
        {
            if (source == null) return null;

            return new Model.MetadataFieldOption
            {
                ID = source.ID,
                Title = source.Title,
                MetadataFieldID = source.MetadataFieldID
            };
        }
    }
}