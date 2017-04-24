## 2.0 - 2017-04-24

- updated to latest Plainion packages
- updated to Prism 6.1
- updated to latest TraceEvent library
- BREAKING CHANGE: Extensibility from "StringTracing" module moved to "Streams" module. Custom implementations now link to "Streams.Sdk" library only.
  IStringTracingParser has been renamed to IStreamTraceParser and its API has been simplified.
- Show summary flames ("True" flame graph)
- ETW: show flames based on events with "start"/"stop" opcode

## 1.0 - 2015-04-25

- Initial release