# Sprint 3 Release Notes

- Model refresh caches the installed list hash to avoid unnecessary redraws in the Models tab.
- Warmup manager now cancels superseded requests, reuses in-flight warmups, and surfaces Ready/Failed events for smoother status updates.
- Settings persistence only writes when the serialized payload changes, cutting redundant disk activity during idle sessions.
