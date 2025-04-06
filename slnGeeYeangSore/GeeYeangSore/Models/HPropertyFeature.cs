using System;
using System.Collections.Generic;

namespace GeeYeangSore.Models;

public partial class HPropertyFeature
{
    public int HFeaturePropertyId { get; set; }

    public int HLandlordId { get; set; }

    public int HPropertyId { get; set; }

    public bool? HAllowsDogs { get; set; }

    public bool? HAllowsCats { get; set; }

    public bool? HAllowsAnimals { get; set; }

    public bool? HAllowsCooking { get; set; }

    public bool? HHasFurniture { get; set; }

    public bool? HInternet { get; set; }

    public bool? HAirConditioning { get; set; }

    public bool? HSharedRental { get; set; }

    public bool? HTv { get; set; }

    public bool? HRefrigerator { get; set; }

    public bool? HWashingMachine { get; set; }

    public bool? HBed { get; set; }

    public bool? HWaterHeater { get; set; }

    public bool? HGasStove { get; set; }

    public bool? HCableTv { get; set; }

    public bool? HWaterDispenser { get; set; }

    public bool? HParking { get; set; }

    public bool? HSocialHousing { get; set; }

    public bool? HShortTermRent { get; set; }

    public bool? HPublicElectricity { get; set; }

    public bool? HPublicWatercharges { get; set; }

    public bool? HLandlordShared { get; set; }

    public bool? HBalcony { get; set; }

    public bool? HPublicEquipment { get; set; }

    public virtual HLandlord HLandlord { get; set; } = null!;

    public virtual HProperty HProperty { get; set; } = null!;
}
