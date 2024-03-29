﻿using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using Tavenem.Chemistry.Elements;
using Tavenem.DataStorage;
using Tavenem.Mathematics;

namespace Tavenem.Chemistry;

/// <summary>
/// A <see cref="JsonSerializerContext"/> for <c>Tavenem.Chemistry</c>
/// </summary>
[JsonSerializable(typeof(IIdItem))]
[JsonSerializable(typeof(ElectronConfiguration))]
[JsonSerializable(typeof(Element))]
[JsonSerializable(typeof(Isotope))]
[JsonSerializable(typeof(Orbital))]
[JsonSerializable(typeof(PeriodicTable))]
[JsonSerializable(typeof(Chemical))]
[JsonSerializable(typeof(Composite<double>))]
[JsonSerializable(typeof(Composite<Half>))]
[JsonSerializable(typeof(Composite<float>))]
[JsonSerializable(typeof(Composite<NFloat>))]
[JsonSerializable(typeof(Formula))]
[JsonSerializable(typeof(HomogeneousReference))]
[JsonSerializable(typeof(HomogeneousSubstance))]
[JsonSerializable(typeof(IHomogeneous))]
[JsonSerializable(typeof(IMaterial<double>))]
[JsonSerializable(typeof(IMaterial<Half>))]
[JsonSerializable(typeof(IMaterial<float>))]
[JsonSerializable(typeof(IMaterial<NFloat>))]
[JsonSerializable(typeof(ISubstance))]
[JsonSerializable(typeof(ISubstanceReference))]
[JsonSerializable(typeof(Material<double>))]
[JsonSerializable(typeof(Material<Half>))]
[JsonSerializable(typeof(Material<float>))]
[JsonSerializable(typeof(Material<NFloat>))]
[JsonSerializable(typeof(Mixture))]
[JsonSerializable(typeof(Solution))]
[JsonSerializable(typeof(SubstanceReference))]
[JsonSerializable(typeof(Capsule<double>))]
[JsonSerializable(typeof(Capsule<Half>))]
[JsonSerializable(typeof(Capsule<float>))]
[JsonSerializable(typeof(Capsule<NFloat>))]
[JsonSerializable(typeof(Cone<double>))]
[JsonSerializable(typeof(Cone<Half>))]
[JsonSerializable(typeof(Cone<float>))]
[JsonSerializable(typeof(Cone<NFloat>))]
[JsonSerializable(typeof(Cuboid<double>))]
[JsonSerializable(typeof(Cuboid<Half>))]
[JsonSerializable(typeof(Cuboid<float>))]
[JsonSerializable(typeof(Cuboid<NFloat>))]
[JsonSerializable(typeof(Cylinder<double>))]
[JsonSerializable(typeof(Cylinder<Half>))]
[JsonSerializable(typeof(Cylinder<float>))]
[JsonSerializable(typeof(Cylinder<NFloat>))]
[JsonSerializable(typeof(Ellipsoid<double>))]
[JsonSerializable(typeof(Ellipsoid<Half>))]
[JsonSerializable(typeof(Ellipsoid<float>))]
[JsonSerializable(typeof(Ellipsoid<NFloat>))]
[JsonSerializable(typeof(Frustum<double>))]
[JsonSerializable(typeof(Frustum<Half>))]
[JsonSerializable(typeof(Frustum<float>))]
[JsonSerializable(typeof(Frustum<NFloat>))]
[JsonSerializable(typeof(HollowSphere<double>))]
[JsonSerializable(typeof(HollowSphere<Half>))]
[JsonSerializable(typeof(HollowSphere<float>))]
[JsonSerializable(typeof(HollowSphere<NFloat>))]
[JsonSerializable(typeof(Line<double>))]
[JsonSerializable(typeof(Line<Half>))]
[JsonSerializable(typeof(Line<float>))]
[JsonSerializable(typeof(Line<NFloat>))]
[JsonSerializable(typeof(SinglePoint<double>))]
[JsonSerializable(typeof(SinglePoint<Half>))]
[JsonSerializable(typeof(SinglePoint<float>))]
[JsonSerializable(typeof(SinglePoint<NFloat>))]
[JsonSerializable(typeof(Sphere<double>))]
[JsonSerializable(typeof(Sphere<Half>))]
[JsonSerializable(typeof(Sphere<float>))]
[JsonSerializable(typeof(Sphere<NFloat>))]
[JsonSerializable(typeof(Torus<double>))]
[JsonSerializable(typeof(Torus<Half>))]
[JsonSerializable(typeof(Torus<float>))]
[JsonSerializable(typeof(Torus<NFloat>))]
public partial class ChemistrySourceGenerationContext
    : JsonSerializerContext;
