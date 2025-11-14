using System.Collections.Generic;

public class ElectricPowerData
{
    public bool init;
    public SortedSet<ElectricalAppliance> connectedAppliances = new();
    public State power = new();
}