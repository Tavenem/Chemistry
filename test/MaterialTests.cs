using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry.Test;

[TestClass]
public class MaterialTests
{
    [TestMethod]
    public void ContainsTest()
    {
        var material = new Material<double>(Substances.All.Water, new Sphere<double>(1), 300);
        Assert.IsTrue(material.Contains(Substances.All.Water));
        Assert.IsTrue(material.Contains(Substances.All.Water, PhaseType.Liquid, 300, 101.325));
    }

    [TestMethod]
    public void ProportionTest()
    {
        var material = new Material<double>(new Sphere<double>(1), null, null, (Substances.All.Water, 0.5m), (Substances.All.Seawater, 0.5m));

        Assert.AreEqual(0.5m, material.GetProportion(Substances.All.Seawater));
    }

    [TestMethod]
    public void SerializationTest()
    {
        var material = new Material<double>(Substances.All.Water, new Sphere<double>(1));

        var json = JsonSerializer.Serialize(material);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(material, JsonSerializer.Deserialize<Material<double>>(json));
        Assert.AreEqual(material, JsonSerializer.Deserialize<IMaterial<double>>(json));

        IMaterial<double> imaterial = material;

        json = JsonSerializer.Serialize(imaterial, imaterial.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(imaterial, JsonSerializer.Deserialize<IMaterial<double>>(json));

        material.Temperature = 5;

        json = JsonSerializer.Serialize(material);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(material, JsonSerializer.Deserialize<Material<double>>(json));
        Assert.AreEqual(material, JsonSerializer.Deserialize<IMaterial<double>>(json));

        imaterial = material;

        json = JsonSerializer.Serialize(imaterial, imaterial.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(imaterial, JsonSerializer.Deserialize<IMaterial<double>>(json));

        var composite = new Composite<double>(new Cuboid<double>(1, 1, 1), components: material);

        json = JsonSerializer.Serialize(composite);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(composite, JsonSerializer.Deserialize<Composite<double>>(json));
        Assert.AreEqual(composite, JsonSerializer.Deserialize<IMaterial<double>>(json));

        imaterial = composite;

        json = JsonSerializer.Serialize(imaterial, imaterial.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(imaterial, JsonSerializer.Deserialize<IMaterial<double>>(json));

        composite.Density = 10;
        composite.Mass = 5000;
        composite.Temperature = 100;

        json = JsonSerializer.Serialize(composite);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(composite, JsonSerializer.Deserialize<Composite<double>>(json));
        Assert.AreEqual(composite, JsonSerializer.Deserialize<IMaterial<double>>(json));

        imaterial = composite;

        json = JsonSerializer.Serialize(imaterial, imaterial.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(imaterial, JsonSerializer.Deserialize<IMaterial<double>>(json));
    }

    [TestMethod]
    public void SplitTest()
    {
        IMaterial<double> material = new Material<double>(new Sphere<double>(1), 300, null, null, (Substances.All.Water, 0.5m), (Substances.All.Benzene, 0.5m));
        material = material.Split(0.8);
        Assert.AreEqual(0.5m, Math.Round(material.GetProportion(Substances.All.Water), 6));
    }
}
