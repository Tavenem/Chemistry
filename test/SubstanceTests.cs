using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tavenem.Chemistry.Test;

[TestClass]
public class SubstanceTests
{
    [TestMethod]
    public void FormulaTest()
    {
        var formula = Formula.Parse("H2O");
        Assert.IsNotNull(formula);
        Assert.AreEqual(3, formula.NumberOfAtoms);
        Assert.AreEqual(0, formula.Charge);
        Assert.IsNotNull(formula.Nuclides);
        var nuclides = formula.Nuclides.ToList();
        Assert.AreEqual(2, nuclides.Count);
        Assert.AreEqual(Elements.PeriodicTable.Instance[1].GetCommonIsotope(), nuclides[0].nuclide);
        Assert.AreEqual(2, nuclides[0].amount);
        Assert.AreEqual(Elements.PeriodicTable.Instance[8].GetCommonIsotope(), nuclides[1].nuclide);
        Assert.AreEqual(1, nuclides[1].amount);

        var str = formula.ToString();
        Assert.AreEqual("H₂O", str);
        Assert.AreEqual(formula, Formula.Parse(str));

        var json = System.Text.Json.JsonSerializer.Serialize(formula);
        Assert.AreEqual("\"H\\u2082O\"", json);
        Assert.AreEqual(formula, System.Text.Json.JsonSerializer.Deserialize<Formula>(json));

        formula = Formula.Parse("SO4-2");
        Assert.IsNotNull(formula);
        Assert.AreEqual(5, formula.NumberOfAtoms);
        Assert.AreEqual(-2, formula.Charge);
        Assert.IsNotNull(formula.Nuclides);
        nuclides = formula.Nuclides.ToList();
        Assert.AreEqual(2, nuclides.Count);
        Assert.AreEqual(Elements.PeriodicTable.Instance[16].GetCommonIsotope(), nuclides[0].nuclide);
        Assert.AreEqual(1, nuclides[0].amount);
        Assert.AreEqual(Elements.PeriodicTable.Instance[8].GetCommonIsotope(), nuclides[1].nuclide);
        Assert.AreEqual(4, nuclides[1].amount);

        str = formula.ToString();
        Assert.AreEqual("O₄S²⁻", str);
        Assert.AreEqual(formula, Formula.Parse(str));

        json = System.Text.Json.JsonSerializer.Serialize(formula);
        Assert.AreEqual("\"O\\u2084S\\u00B2\\u207B\"", json);
        Assert.AreEqual(formula, System.Text.Json.JsonSerializer.Deserialize<Formula>(json));
    }

    [TestMethod]
    public void EqualityTest()
    {
        var reference = Substances.All.Water.GetReference();
        Assert.IsTrue(reference.Equals(Substances.All.Water));
        Assert.IsTrue(Substances.All.Water.Equals(reference));
    }

    [TestMethod]
    public void PhaseTest()
    {
        Assert.AreEqual(PhaseType.Solid, Substances.All.Water.GetPhase(179.29430552554183, 1.1673449850263058));
        Assert.AreEqual(PhaseType.Solid, Substances.All.Seawater.GetPhase(179.29430552554183, 1.1673449850263058));
        Assert.AreEqual(PhaseType.Solid, Substances.All.Water.GetPhase(76.245536697746417, 1.1673449850263058));
        Assert.AreEqual(PhaseType.Solid, Substances.All.Seawater.GetPhase(76.245536697746417, 1.1673449850263058));
        Assert.AreEqual(PhaseType.Solid, Substances.All.Water.GetPhase(175.97883428221979, 1.1673449850263058));
        Assert.AreEqual(PhaseType.Solid, Substances.All.Seawater.GetPhase(175.97883428221979, 1.1673449850263058));
    }

    [TestMethod]
    public void Serialization_ChemicalTest()
    {
        var chemical = Substances.All.Water;

        var json = System.Text.Json.JsonSerializer.Serialize(chemical);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = System.Text.Json.JsonSerializer.Deserialize<Chemical>(json);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(chemical.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(chemical.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(chemical, deserialized);
        Assert.AreEqual(chemical, System.Text.Json.JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(chemical, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));

        IHomogeneous iHomogeneous = chemical;

        json = System.Text.Json.JsonSerializer.Serialize(iHomogeneous, iHomogeneous.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(iHomogeneous, System.Text.Json.JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(iHomogeneous, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));

        ISubstance iSubstance = chemical;

        json = System.Text.Json.JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(iSubstance, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));
    }

    [TestMethod]
    public void Serialization_HomogeneousReferenceTest()
    {
        var homogeneousReference = Substances.All.Water.GetHomogeneousReference();

        var json = System.Text.Json.JsonSerializer.Serialize(homogeneousReference, homogeneousReference.GetType());
        Assert.AreEqual($"\"HR:{homogeneousReference.Id}\"", json);
        Assert.AreEqual(homogeneousReference, System.Text.Json.JsonSerializer.Deserialize<HomogeneousReference>(json));
        Assert.AreEqual(homogeneousReference, System.Text.Json.JsonSerializer.Deserialize<ISubstanceReference>(json));

        ISubstanceReference iSubstanceReference = homogeneousReference;

        json = System.Text.Json.JsonSerializer.Serialize(iSubstanceReference, homogeneousReference.GetType());
        Assert.AreEqual($"\"HR:{iSubstanceReference.Id}\"", json);
        Assert.AreEqual(iSubstanceReference, System.Text.Json.JsonSerializer.Deserialize<ISubstanceReference>(json));
    }

    [TestMethod]
    public void Serialization_HomogeneousSubstanceTest()
    {
        var homogeneousSubstance = Substances.All.Protein;

        var json = System.Text.Json.JsonSerializer.Serialize(homogeneousSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, System.Text.Json.JsonSerializer.Deserialize<HomogeneousSubstance>(json));
        Assert.AreEqual(homogeneousSubstance, System.Text.Json.JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(homogeneousSubstance, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));

        IHomogeneous iHomogeneous = homogeneousSubstance;

        json = System.Text.Json.JsonSerializer.Serialize(iHomogeneous, iHomogeneous.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(iHomogeneous, System.Text.Json.JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(iHomogeneous, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));

        ISubstance iSubstance = homogeneousSubstance;

        json = System.Text.Json.JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(iSubstance, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));
    }

    [TestMethod]
    public void Serialization_MixtureTest()
    {
        var mixture = Substances.All.Basalt;

        var json = System.Text.Json.JsonSerializer.Serialize(mixture);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, System.Text.Json.JsonSerializer.Deserialize<Mixture>(json));
        Assert.AreEqual(mixture, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));

        ISubstance iSubstance = mixture;

        json = System.Text.Json.JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(iSubstance, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));
    }

    [TestMethod]
    public void Serialization_SubstanceReferenceTest()
    {
        var substanceReference = Substances.All.Basalt.GetReference();

        var json = System.Text.Json.JsonSerializer.Serialize(substanceReference, substanceReference.GetType());
        Assert.AreEqual($"\"SR:{substanceReference.Id}\"", json);
        Assert.AreEqual(substanceReference, System.Text.Json.JsonSerializer.Deserialize<SubstanceReference>(json));
        Assert.AreEqual(substanceReference, System.Text.Json.JsonSerializer.Deserialize<ISubstanceReference>(json));
    }

    [TestMethod]
    public void Serialization_SolutionTest()
    {
        var solution = Substances.All.Seawater;

        var json = System.Text.Json.JsonSerializer.Serialize(solution);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, System.Text.Json.JsonSerializer.Deserialize<Solution>(json));
        Assert.AreEqual(solution, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));

        ISubstance iSubstance = solution;

        json = System.Text.Json.JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(iSubstance, System.Text.Json.JsonSerializer.Deserialize<ISubstance>(json));
    }
}
