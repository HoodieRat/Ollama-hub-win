# Modernization Roadmap

| Sprint | Goals | Key Tasks | Expected Impact |
| --- | --- | --- | --- |
| Sprint 1 | Establish structure for maintainability | • Split `Form1` into partial classes for UI wiring, model logic, persistence, and diagnostics<br>• Remove duplicate citation regex definitions and reference a single shared instance<br>• Capture follow-up refactoring notes in code comments where responsibilities shift | Lower cognitive load when navigating the form code; quick win on readability |
| Sprint 2 | Promote reuse and resilience | • Extract shared options/preset utilities and warmup helpers into dedicated services<br>• Introduce a warmup manager that debounces requests and centralizes cancellation<br>• Add lightweight unit coverage around the new services | Consistent behavior between form and tuner; fewer redundant warmups |
| Sprint 3 | Optimize runtime behavior | • Convert settings/session file I/O to asynchronous APIs with proper context capture<br>• Add model list diffs to skip redundant refreshes<br>• Replace diagnostics list churn with a ring buffer<br>• Avoid unnecessary settings writes when data is unchanged | Smoother UI responsiveness and reduced background churn |
