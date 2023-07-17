using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;
using Tavenem.DataStorage;

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

        var json = JsonSerializer.Serialize(formula);
        Assert.AreEqual("\"H\\u2082O\"", json);
        Assert.AreEqual(formula, JsonSerializer.Deserialize<Formula>(json));

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

        json = JsonSerializer.Serialize(formula);
        Assert.AreEqual("\"O\\u2084S\\u00B2\\u207B\"", json);
        Assert.AreEqual(formula, JsonSerializer.Deserialize<Formula>(json));
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
        IHomogeneous iHomogeneous = chemical;
        ISubstance iSubstance = chemical;
        IIdItem iIdItem = chemical;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };

        var json = JsonSerializer.Serialize(chemical);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<Chemical>(json);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(chemical.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(chemical.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(chemical, deserialized);

        json = JsonSerializer.Serialize(iHomogeneous, iHomogeneous.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<Chemical>(json));

        json = JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<Chemical>(json));

        json = JsonSerializer.Serialize(iIdItem, iIdItem.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<Chemical>(json));

        json = JsonSerializer.Serialize(iHomogeneous);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<Chemical>(json));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<ISubstance>(json));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<Chemical>(json));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<ISubstance>(json));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<Chemical>(json, options));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IHomogeneous>(json, options));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<ISubstance>(json, options));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IIdItem>(json, options));

        options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };
        options.TypeInfoResolverChain.Add(ChemistrySourceGenerationContext.Default);

        json = JsonSerializer.Serialize(chemical, ChemistrySourceGenerationContext.Default.Chemical);
        Console.WriteLine();
        Console.WriteLine(json);
        deserialized = JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Chemical);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(chemical.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(chemical.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(chemical, deserialized);

        json = JsonSerializer.Serialize(iHomogeneous, ChemistrySourceGenerationContext.Default.Chemical);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Chemical));

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.Chemical);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Chemical));

        json = JsonSerializer.Serialize(iIdItem, ChemistrySourceGenerationContext.Default.Chemical);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Chemical));

        json = JsonSerializer.Serialize(iHomogeneous, ChemistrySourceGenerationContext.Default.IHomogeneous);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Chemical));
        Assert.AreEqual(
            chemical,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.IHomogeneous));
        Assert.AreEqual(
            chemical,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.ISubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Chemical));
        Assert.AreEqual(
            chemical,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.IHomogeneous));
        Assert.AreEqual(
            chemical,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(chemical, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Chemical));
        Assert.AreEqual(
            chemical,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.IHomogeneous));
        Assert.AreEqual(
            chemical,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(chemical, JsonSerializer.Deserialize<IIdItem>(json, options));
    }

    [TestMethod]
    public void Serialization_HomogeneousReferenceTest()
    {
        var homogeneousReference = Substances.All.Water.GetHomogeneousReference();
        ISubstanceReference iSubstanceReference = homogeneousReference;

        var json = JsonSerializer.Serialize(homogeneousReference, homogeneousReference.GetType());
        Assert.AreEqual($"\"HR:{homogeneousReference.Id}\"", json);
        Assert.AreEqual(homogeneousReference, JsonSerializer.Deserialize<HomogeneousReference>(json));

        json = JsonSerializer.Serialize(iSubstanceReference, iSubstanceReference.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousReference, JsonSerializer.Deserialize<HomogeneousReference>(json));

        json = JsonSerializer.Serialize(iSubstanceReference);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousReference, JsonSerializer.Deserialize<HomogeneousReference>(json));
        Assert.AreEqual(homogeneousReference, JsonSerializer.Deserialize<ISubstanceReference>(json));

        json = JsonSerializer.Serialize(homogeneousReference, ChemistrySourceGenerationContext.Default.HomogeneousReference);
        Assert.AreEqual($"\"HR:{homogeneousReference.Id}\"", json);
        Assert.AreEqual(
            homogeneousReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousReference));

        json = JsonSerializer.Serialize(iSubstanceReference, ChemistrySourceGenerationContext.Default.ISubstanceReference);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(
            homogeneousReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousReference));

        json = JsonSerializer.Serialize(iSubstanceReference, ChemistrySourceGenerationContext.Default.ISubstanceReference);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(
            homogeneousReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousReference));
        Assert.AreEqual(
            homogeneousReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstanceReference));
    }

    [TestMethod]
    public void Serialization_HomogeneousSubstanceTest()
    {
        var homogeneousSubstance = Substances.All.Protein;
        IHomogeneous iHomogeneous = homogeneousSubstance;
        ISubstance iSubstance = homogeneousSubstance;
        IIdItem iIdItem = homogeneousSubstance;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };

        var json = JsonSerializer.Serialize(homogeneousSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<HomogeneousSubstance>(json);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(homogeneousSubstance.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(homogeneousSubstance.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(homogeneousSubstance, deserialized);

        json = JsonSerializer.Serialize(iHomogeneous, iHomogeneous.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<HomogeneousSubstance>(json));

        json = JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<HomogeneousSubstance>(json));

        json = JsonSerializer.Serialize(iIdItem, iIdItem.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<HomogeneousSubstance>(json));

        json = JsonSerializer.Serialize(iHomogeneous);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<HomogeneousSubstance>(json));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<ISubstance>(json));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<HomogeneousSubstance>(json));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IHomogeneous>(json));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<ISubstance>(json));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<HomogeneousSubstance>(json, options));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IHomogeneous>(json, options));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<ISubstance>(json, options));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IIdItem>(json, options));

        options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };
        options.TypeInfoResolverChain.Add(ChemistrySourceGenerationContext.Default);

        json = JsonSerializer.Serialize(homogeneousSubstance, ChemistrySourceGenerationContext.Default.HomogeneousSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        deserialized = JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousSubstance);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(homogeneousSubstance.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(homogeneousSubstance.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(homogeneousSubstance, deserialized);

        json = JsonSerializer.Serialize(iHomogeneous, ChemistrySourceGenerationContext.Default.HomogeneousSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousSubstance));

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.HomogeneousSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousSubstance));

        json = JsonSerializer.Serialize(iIdItem, ChemistrySourceGenerationContext.Default.HomogeneousSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousSubstance));

        json = JsonSerializer.Serialize(iHomogeneous, ChemistrySourceGenerationContext.Default.IHomogeneous);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousSubstance));
        Assert.AreEqual(
            homogeneousSubstance,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.IHomogeneous));
        Assert.AreEqual(
            homogeneousSubstance,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.ISubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousSubstance));
        Assert.AreEqual(
            homogeneousSubstance,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.IHomogeneous));
        Assert.AreEqual(
            homogeneousSubstance,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.HomogeneousSubstance));
        Assert.AreEqual(
            homogeneousSubstance,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.IHomogeneous));
        Assert.AreEqual(
            homogeneousSubstance,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(homogeneousSubstance, JsonSerializer.Deserialize<IIdItem>(json, options));
    }

    [TestMethod]
    public void Serialization_MixtureTest()
    {
        var mixture = Substances.All.Basalt;
        ISubstance iSubstance = mixture;
        IIdItem iIdItem = mixture;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };

        var json = JsonSerializer.Serialize(mixture);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<Mixture>(json);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(mixture.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(mixture.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(mixture, deserialized);

        json = JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<Mixture>(json));

        json = JsonSerializer.Serialize(iIdItem, iIdItem.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<Mixture>(json));

        json = JsonSerializer.Serialize(iSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<Mixture>(json));
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<ISubstance>(json));
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<Mixture>(json, options));
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<ISubstance>(json, options));
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<IIdItem>(json, options));

        options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };
        options.TypeInfoResolverChain.Add(ChemistrySourceGenerationContext.Default);

        json = JsonSerializer.Serialize(mixture, ChemistrySourceGenerationContext.Default.Mixture);
        Console.WriteLine();
        Console.WriteLine(json);
        deserialized = JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Mixture);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(mixture.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(mixture.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(mixture, deserialized);

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.Mixture);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Mixture));

        json = JsonSerializer.Serialize(iIdItem, ChemistrySourceGenerationContext.Default.Mixture);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Mixture));

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.ISubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Mixture));
        Assert.AreEqual(
            mixture,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(mixture, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Mixture));
        Assert.AreEqual(
            mixture,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(mixture, JsonSerializer.Deserialize<IIdItem>(json, options));
    }

    [TestMethod]
    public void Serialization_SubstanceReferenceTest()
    {
        var substanceReference = new SubstanceReference(Substances.All.Basalt);
        ISubstanceReference iSubstanceReference = substanceReference;

        var json = JsonSerializer.Serialize(substanceReference, substanceReference.GetType());
        Assert.AreEqual($"\"SR:{substanceReference.Id}\"", json);
        Assert.AreEqual(substanceReference, JsonSerializer.Deserialize<SubstanceReference>(json));

        json = JsonSerializer.Serialize(iSubstanceReference, iSubstanceReference.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(substanceReference, JsonSerializer.Deserialize<SubstanceReference>(json));

        json = JsonSerializer.Serialize(iSubstanceReference);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(substanceReference, JsonSerializer.Deserialize<SubstanceReference>(json));
        Assert.AreEqual(substanceReference, JsonSerializer.Deserialize<ISubstanceReference>(json));

        json = JsonSerializer.Serialize(substanceReference, ChemistrySourceGenerationContext.Default.SubstanceReference);
        Assert.AreEqual($"\"SR:{substanceReference.Id}\"", json);
        Assert.AreEqual(
            substanceReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.SubstanceReference));

        json = JsonSerializer.Serialize(iSubstanceReference, ChemistrySourceGenerationContext.Default.ISubstanceReference);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(
            substanceReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.SubstanceReference));

        json = JsonSerializer.Serialize(iSubstanceReference, ChemistrySourceGenerationContext.Default.ISubstanceReference);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(
            substanceReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.SubstanceReference));
        Assert.AreEqual(
            substanceReference,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstanceReference));
    }

    [TestMethod]
    public void Serialization_SolutionTest()
    {
        var solution = Substances.All.Seawater;
        ISubstance iSubstance = solution;
        IIdItem iIdItem = solution;

        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };

        var json = JsonSerializer.Serialize(solution);
        Console.WriteLine();
        Console.WriteLine(json);
        var deserialized = JsonSerializer.Deserialize<Solution>(json);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(solution.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(solution.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(solution, deserialized);

        json = JsonSerializer.Serialize(iSubstance, iSubstance.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize<Solution>(json));

        json = JsonSerializer.Serialize(iIdItem, iIdItem.GetType());
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize<Solution>(json));

        json = JsonSerializer.Serialize(iSubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize<Solution>(json));
        Assert.AreEqual(solution, JsonSerializer.Deserialize<ISubstance>(json));
        Assert.AreEqual(solution, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize<Solution>(json, options));
        Assert.AreEqual(solution, JsonSerializer.Deserialize<ISubstance>(json, options));
        Assert.AreEqual(solution, JsonSerializer.Deserialize<IIdItem>(json, options));

        options = new JsonSerializerOptions
        {
            TypeInfoResolver = new ChemistryTypeResolver()
        };
        options.TypeInfoResolverChain.Add(ChemistrySourceGenerationContext.Default);

        json = JsonSerializer.Serialize(solution, ChemistrySourceGenerationContext.Default.Solution);
        Console.WriteLine();
        Console.WriteLine(json);
        deserialized = JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Solution);
        Assert.IsNotNull(deserialized);
        Assert.IsNotNull(solution.Categories);
        Assert.IsNotNull(deserialized.Categories);
        Assert.IsTrue(solution.Categories.SequenceEqual(deserialized.Categories));
        Assert.AreEqual(solution, deserialized);

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.Solution);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Solution));

        json = JsonSerializer.Serialize(iIdItem, ChemistrySourceGenerationContext.Default.Solution);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Solution));

        json = JsonSerializer.Serialize(iSubstance, ChemistrySourceGenerationContext.Default.ISubstance);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Solution));
        Assert.AreEqual(
            solution,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(solution, JsonSerializer.Deserialize<IIdItem>(json, options));

        json = JsonSerializer.Serialize(iIdItem, options);
        Console.WriteLine();
        Console.WriteLine(json);
        Assert.AreEqual(solution, JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.Solution));
        Assert.AreEqual(
            solution,
            JsonSerializer.Deserialize(json, ChemistrySourceGenerationContext.Default.ISubstance));
        Assert.AreEqual(solution, JsonSerializer.Deserialize<IIdItem>(json, options));
    }
}
