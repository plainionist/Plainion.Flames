---
title: Contributing &amp; Extending
navigation_weight: 1
---

# Extensibility

Plainion.Flames supports writing custom plugins to create flames from custom semantic traces.

Just download the source and have a look at Plainion.Flames.Modules.Streams to see how to 
integrate plugins into Plainion.GraphViz. Inside the custom plugin just

- Implementing "IStreamTraceParser" (you need to link to Plainion.Flames.Modules.Streams.Sdk)
- export it via MEF
- put it into an assembly with the naming convention: "Plainion.Flames.Modules.Streams.*.dll"
- and put this into the folder where you unpacked Plainion.Flames

Plainion.Flames will then visualize your custom semantic traces!

