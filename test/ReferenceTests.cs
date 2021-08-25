using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.ComponentModel;

namespace Tavenem.Chemistry.Test;

[TestClass]
public class ReferenceTests
{
    [TestMethod]
    public void ChemicalReferenceConversionTest()
    {
        var reference = Substances.All.Water;
        var str = TypeDescriptor.GetConverter(reference.GetType()).ConvertToString(reference);
        Assert.IsNotNull(str);
        var deserialized = TypeDescriptor.GetConverter(reference.GetType()).ConvertFromString(str);
        Assert.IsTrue(reference.Equals(deserialized));
    }

    [TestMethod]
    public void GetReferenceTest()
    {
        var reference = Substances.All.Oxygen.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.Ammonia.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.AmmoniumHydrosulfide.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.CarbonMonoxide.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.CarbonDioxide.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.HydrogenSulfide.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.Methane.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.Ozone.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.Phosphine.GetHomogeneousReference();
        Assert.IsNotNull(reference);
        reference = Substances.All.SulphurDioxide.GetHomogeneousReference();
        Assert.IsNotNull(reference);
    }

    [TestMethod]
    public void HomogeneousReferenceConversionTest()
    {
        var reference = Substances.All.Seawater;
        var str = TypeDescriptor.GetConverter(reference.GetType()).ConvertToString(reference);
        Assert.IsNotNull(str);
        var deserialized = TypeDescriptor.GetConverter(reference.GetType()).ConvertFromString(str);
        Assert.IsTrue(reference.Equals(deserialized));
    }

    [TestMethod]
    public void SubstanceReferenceConversionTest()
    {
        var reference = Substances.All.Basalt;
        var str = TypeDescriptor.GetConverter(reference.GetType()).ConvertToString(reference);
        Assert.IsNotNull(str);
        var deserialized = TypeDescriptor.GetConverter(reference.GetType()).ConvertFromString(str);
        Assert.IsTrue(reference.Equals(deserialized));
    }

    [TestMethod]
    public void IHomogeneousReferenceConversionTest()
    {
        var reference = Substances.All.Seawater.GetHomogeneousReference();
        var str = TypeDescriptor.GetConverter(reference.GetType()).ConvertToString(reference);
        Assert.IsNotNull(str);
        var deserialized = TypeDescriptor.GetConverter(reference.GetType()).ConvertFromString(str);
        Assert.IsTrue(reference.Equals(deserialized));
    }

    [TestMethod]
    public void ISubstanceReferenceConversionTest()
    {
        var reference = Substances.All.Basalt.GetReference();
        var str = TypeDescriptor.GetConverter(reference.GetType()).ConvertToString(reference);
        Assert.IsNotNull(str);
        var deserialized = TypeDescriptor.GetConverter(reference.GetType()).ConvertFromString(str);
        Assert.IsTrue(reference.Equals(deserialized));
    }

    [TestMethod]
    public void BoxedChemicalReferenceConversionTest()
    {
        var reference = Substances.All.Water.GetHomogeneousReference();
        var str = TypeDescriptor.GetConverter(reference.GetType()).ConvertToString(reference);
        Assert.IsNotNull(str);
        var deserialized = TypeDescriptor.GetConverter(reference.GetType()).ConvertFromString(str);
        Assert.IsTrue(reference.Equals(deserialized));

        ISubstanceReference reference2 = reference;
        str = TypeDescriptor.GetConverter(reference2.GetType()).ConvertToString(reference2);
        Assert.IsNotNull(str);
        deserialized = TypeDescriptor.GetConverter(reference2.GetType()).ConvertFromString(str);
        Assert.IsTrue(reference2.Equals(deserialized));
    }
}
