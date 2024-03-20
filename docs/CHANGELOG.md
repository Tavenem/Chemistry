# Changelog

## 1.1
### Changed
- Use `JsonTypeInfo` for registration

## 1.0
### Added
- Initial production release

## 0.9.1-preview
### Added
- Support the use of `HomogeneousReference` and `ISubstanceReference` as dictionary keys in JSON (de)serialization

## 0.9.0-preview
### Removed
- `Empty` property in `Material`, which was a static, mutable instance. Uses should be replaced by `new Material<T>()`

## 0.8.0-preview
### Added
- `GetAll` in `Substances.All`
### Updated
- Update to .NET 8 preview

## 0.7.0-preview
### Added
- Categories
### Changed
- Init properties
### Removed
- IsGemstone (replaced by a category)

## 0.6.0-preview
### Added
- Common names
- Substance enumerations

## 0.5.2-preview
### Updated
- Update to .NET 7

## 0.5.1-preview
### Updated
- Update dependencies

## 0.5.0-preview
### Updated
- Update to .NET 7 RC
- Remove dependency on preview features
### Changed
- Use native polymorphic (de)serialization for `ISubstance` and `IHomogenousSubstance`

## 0.4.1-preview
### Updated
- Update dependencies

## 0.4.0-preview
### Changed
- Library substances will now have persistent Ids, which do not change between executions or library revisions.

## 0.3.5-preview
### Fixed
- Excessive scope for `Material` extensions

## 0.3.4-preview
### Fixed
- `Material` constructor

## 0.3.2-preview - 0.3.3-preview
### Updated
- Update dependencies

## 0.3.1-preview
### Changed
- Add `JsonPropertyOrder` to some elements

## 0.3.0-preview
### Changed
- Re-unify `IMaterial`, `Material`, and `Composite` using static abstract interfaces
- Make structs read-only
### Removed
- Remove support for non-JSON serialization

## 0.2.0-preview
### Changed
- Separate `IMaterial`, `Material`, and `Composite` into `decimal`, `double`, and `HugeNumber`
  variants (mirrors [Tavenem.Mathematics](https://github.com/Tavenem/Mathematics) structure). The
  variants use these datatypes for `Mass` and `Shape`, but other properties use either `decimal` or
  `double` for all variants (e.g. proportions always use `decimal` and density always uses
  `double`).
- Simplify constructor signatures for `Material`.
- Simplify signature of `IMaterial` extension method `ScaleShape`.

## 0.1.6-preview
### Fixed
- Corrected values for chitin

## 0.1.5-preview
### Fixed
- Corrected values for cement

## 0.1.4-preview
### Fixed
- Fix `Composite` JSON (de)serialization

## 0.1.1-preview - 0.1.3-preview
### Updated
- Update dependencies

## 0.1.0-preview
### Added
- Initial preview release