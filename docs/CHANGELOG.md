# Changelog

## 0.3.2-preview - 0.3.3-preview
### Updated
- Update dependencies

## 0.3.1-preview
### Changed
- Add `JsonPropertyOrder` to some elements

## 0.3.0-preview
### Changed
- Re-unify `IMaterial`, `Material`, and `Composite` using static abstract interfaces
- Make structs readonly
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