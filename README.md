# Plainion.Flames

Plainion.Flames visualizes the control flow of processes and threads in similar way as a [FlameGraph](http://www.brendangregg.com/FlameGraphs/cpuflamegraphs.html).

It supports string based semantic application traces (you just need to implement a simple line based parser) and ETW traces (CPU sampling and CSwitch events).

You can then navigate and filter the flames e.g. for gaining a deep understanding of the control flow or for gaining an overview for further detailed performance investigations.


![](doc/Screenshots/Flames.Overview.png)

![](doc/Screenshots/Flames.Filter.png)


