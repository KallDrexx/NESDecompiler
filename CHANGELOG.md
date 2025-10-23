# Changelog
All notable changes to this project will be documented in this file.

The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),  
and this project adheres to [Semantic Versioning](https://semver.org/).

---

## [v1.1.1] - 2025-10-18
### Added
- Decompilation can now start from the **first byte of a code region** (#12).

### Changed
- Invalid instructions are now considered the **end of a function trace** (#11).
- Improved stability of function tracing and edge-case instruction handling.

### Fixed
- Minor internal decompiler logic bugs.
- General performance and reliability improvements across builds.

### Notes
- All Windows builds now include both GUI and CLI versions.
- Linux and macOS builds are CLI-only.
- All binaries are **self-contained** and **do not require a .NET runtime**.

**Contributors:**  
[@KallDrexx](https://github.com/KallDrexx)

---

## [v1.1.0] - 2025-10-12
### Added
- Implemented **single function decompiler** with proper tracing.
- Added support for **sub-address instructions** and **virtual instructions** (#10).
- Added **CI/CD workflow** (`build-release.yml`) for automated builds and packaging.
- Added **multiple platform releases**:
  - Windows (x64, x86, ARM64) with GUI + CLI  
  - Linux (x64, ARM64) CLI  
  - macOS (x64, ARM64) CLI

### Changed
- Improved function discovery to handle **wraparound and disassembly boundaries** (#9).
- Reworked `ToString()` formatting for instructions for clarity.
- Improved tracing logic for **unreferenced instruction analysis** (#6).
- Decompiler now directly jumps to instructions that appear within other instructions.

### Fixed
- Fixed incorrect ordering of instructions in output.
- Fixed 16KB ROMs not decompiling (#7).
- Fixed nullability warnings.
- Fixed various stability issues in the decompiler core.

**Contributors:**  
[@ApfelTeeSaft](https://github.com/ApfelTeeSaft), [@KallDrexx](https://github.com/KallDrexx)

---

## [v1.0.0] - 2025-05-15
### Added
- **Initial release** of the NES Decompiler.
- Included both **CLI** and **GUI** builds for Windows (x64).
- Added base decompilation engine and ROM handling logic.
- Added initial README and documentation.

**Contributors:**  
[@ApfelTeeSaft](https://github.com/ApfelTeeSaft)

---

## [Unreleased]
- Planned improvements to function boundary detection.
- Optimizations for recursive instruction analysis.
- Additional architecture support under evaluation.
