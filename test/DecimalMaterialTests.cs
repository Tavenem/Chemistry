using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Tavenem.Mathematics.Decimals;
using Tavenem.Chemistry.Decimals;

namespace Tavenem.Chemistry.Test
{
    [TestClass]
    public class DecimalMaterialTests
    {
        [TestMethod]
        public void ContainsTest()
        {
            var material = new Material(Substances.All.Water, new Sphere(1), 300);
            Assert.IsTrue(material.Contains(Substances.All.Water));
            Assert.IsTrue(material.Contains(Substances.All.Water, PhaseType.Liquid, 300, 101.325));
        }

        [TestMethod]
        public void ProportionTest()
        {
            var material = new Material(new Sphere(1), null, null, null, (Substances.All.Water, 0.5m), (Substances.All.Seawater, 0.5m));

            Assert.AreEqual(0.5m, material.GetProportion(Substances.All.Seawater));
        }

        [TestMethod]
        public void SerializationTest()
        {
            var material = new Material(Substances.All.Water, new Sphere(1));

            var json = System.Text.Json.JsonSerializer.Serialize(material);
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(material, System.Text.Json.JsonSerializer.Deserialize<Material>(json));
            Assert.AreEqual(material, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));

            IMaterial imaterial = material;

            json = System.Text.Json.JsonSerializer.Serialize(imaterial, imaterial.GetType());
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(imaterial, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));

            material.Temperature = 5;

            json = System.Text.Json.JsonSerializer.Serialize(material);
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(material, System.Text.Json.JsonSerializer.Deserialize<Material>(json));
            Assert.AreEqual(material, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));

            imaterial = material;

            json = System.Text.Json.JsonSerializer.Serialize(imaterial, imaterial.GetType());
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(imaterial, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));

            var composite = new Composite(new Cuboid(1, 1, 1), components: material);

            json = System.Text.Json.JsonSerializer.Serialize(composite);
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(composite, System.Text.Json.JsonSerializer.Deserialize<Composite>(json));
            Assert.AreEqual(composite, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));

            imaterial = composite;

            json = System.Text.Json.JsonSerializer.Serialize(imaterial, imaterial.GetType());
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(imaterial, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));

            composite.Density = 10;
            composite.Mass = 5000;
            composite.Temperature = 100;

            json = System.Text.Json.JsonSerializer.Serialize(composite);
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(composite, System.Text.Json.JsonSerializer.Deserialize<Composite>(json));
            Assert.AreEqual(composite, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));

            imaterial = composite;

            json = System.Text.Json.JsonSerializer.Serialize(imaterial, imaterial.GetType());
            Console.WriteLine();
            Console.WriteLine(json);
            Assert.AreEqual(imaterial, System.Text.Json.JsonSerializer.Deserialize<IMaterial>(json));
        }

        [TestMethod]
        public void SplitTest()
        {
            IMaterial material = new Material(new Sphere(1), 300, null, null, (Substances.All.Water, 0.5m), (Substances.All.Benzene, 0.5m));
            material = material.Split(0.8m);
            Assert.AreEqual(0.5m, Math.Round(material.GetProportion(Substances.All.Water), 6));
        }
    }
}
