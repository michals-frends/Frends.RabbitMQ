# Changelog

## [1.1.1] - 2023-03-14
### Fixed
- Fixed issue with connections left open after task's execution by implementing IDisposable in Connectionhelper class.

### Added
- Added support for quorum queues.

## [1.1.0] - 2023-02-23
### Fixed
- Fixed NullReferenceException when no headers are defined for the input object.

## [1.0.1] - 2022-10-12
### Changed
- Unnecessary input check removed.

## [1.0.0] - 2022-08-18
### Added
- Initial implementation