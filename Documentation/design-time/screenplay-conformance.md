---
title: Audit Screenplay conformance
description: Compare an inline Screenplay model with bounded Chronicle metadata from a single sequence.
---

# Audit Screenplay conformance

Use `audit_screenplay_conformance` to compare a self-contained inline or operator-published Screenplay model with bounded evidence from one Chronicle sequence. The tool is read-only in both deployment profiles. Chronicle authorizes every read using the deployment credential, not the MCP caller's model or mutation grants.

## Request

Supply the exact, case-sensitive event store, namespace, and sequence names, plus an inclusive range of nonnegative sequence numbers. The range is clamped to the captured tail. Supply exactly one model input: `modelSource` for inline text or `catalogModel` for an exact published name. For example:

```json
{
  "eventStore": "Library",
  "namespace": "Default",
  "sequence": "event-log",
  "fromSequence": 0,
  "toSequence": 499,
  "modelSource": "domain Library\nmodule Library\n  feature Registration\n    slice StateChange Register\n      command Register\n        id Uuid identifier\n        name String\n        produces Registered\n          for id\n          name = name\n      event Registered\n        name String\n"
}
```

The inline document is treated as `application.play`. Inline input does not support document imports, external system extensions, local paths, or root aliases. Both inputs are limited to 64 KiB of UTF-8 and 4,096 lines across the model, with at most 2,048 bytes per line and 128 spaces of indentation. The optional `applicationName` sets the semantic application identity; otherwise the model domain or configured event store name is used. Target identifiers and the application name are limited to 256 characters.

Use `occurredFrom` and `occurredTo` together to filter by occurrence time (UTC, start inclusive, end exclusive, at most 24 hours). The tool still scans positions in sequence order because occurrence times need not be ordered. It reports missing positions and time exclusions as unexamined. A nonexistent sequence is an error, not empty evidence.

## Publish a model catalog

The catalog is off by default. To opt in, add `ModelCatalog` under `Cratis:Chronicle:Mcp` in the server configuration. For example:

```json
{
  "Cratis": {
    "Chronicle": {
      "Mcp": {
        "ModelCatalog": {
          "Roots": { "release": "/models/library" },
          "Models": {
            "Library": {
              "Root": "release",
              "Entry": "application.play",
              "Files": ["application.play", "registration/register.play"]
            }
          }
        }
      }
    }
  }
}
```

In this example, `/models/library/application.play` imports `registration/register.play`. An audit caller selects the published model by name, not a file path:

```json
{
  "eventStore": "Library",
  "namespace": "Default",
  "sequence": "event-log",
  "fromSequence": 0,
  "toSequence": 499,
  "catalogModel": "Library"
}
```

- Up to 16 root aliases and 64 model names are supported. Names and aliases are case-sensitive, use 1–64 ASCII letters, digits, `_`, or `-`, and cannot differ only in case.
- List every file in `Files`, including the root-level `Entry` (at most 16 files total per model). Use distinct portable relative paths ending in `.play`, with forward slashes, at most 16 components, and at most 240 characters. Absolute paths, backslashes, `.` and `..` components, and wildcards in manifest paths are not allowed.
- Every listed file must be reached from `Entry`. Relative file imports and globs resolve only against the manifest. Rooted imports, traversal, unmatched imports, domain imports, and system extensions are refused. Files and extension artifacts are never auto-discovered.
- Catalog reads require Linux x64 or arm64 with kernel 5.6 or later and a seccomp policy that allows `openat2`. Use an absolute root other than `/`; the root and its ancestors must not be symlinks. In Docker, mount an operator-managed model directory read-only at the configured root (for example, `/models/library`). Files must be regular and cannot be reached through symlinks or nested mounts. No procfs access is required to read them.
- Settings and roots are captured at startup. If a root is missing or unavailable then, restart after correcting it. An unavailable catalog does not disable inline audits. Pinning a directory does not freeze its contents or verify their provenance; use operator-controlled, read-only snapshots for reproducibility.

**All authorized audit callers in either deployment profile can select every published name.** Publish only nonsecret models from operator-controlled trees. Never publish from a shared writable directory.

## Interpret the result

The untrusted result includes the captured tail, examination bounds, gaps, counts, findings, per-slice evidence, and what was not established. For example, a range that starts strictly after the first event can be partial. A modeled relationship with no attributed appends in that range can be `Unknown`: lack of evidence is not evidence that the relationship never runs. Do not interpret an absent `hasDrift` verdict as `false`.

Each call examines at most 2,000 positions. Metadata requests start at up to 500 positions. If Chronicle reports that a response exceeds the receive limit, the tool halves the request at the same position and uses the smaller size for subsequent pages. The scan allows at most 256 metadata attempts within a ten-second deadline. An oversized single record or exhausted retry budget fails the audit without a partial verdict. Only one audit runs at a time per server process.

The audit connection caps each unary response at 16 MiB: a budget of 4 KiB per entry across 4,096 entries, allowing room for event schemas and observer descriptors in unpaged responses. This is an average budget, not a per-entry guarantee. A larger unpaged response causes a read-limit error; narrowing the sequence range cannot shrink it.

If `scanCapped` is true, use the returned `continuation` range with the same target, model, and filters. The next call conducts an independent comparison; it does not extend a previous proof of absence.

`reportTruncated` and the omitted counts describe the separate 64 KiB response budget. The tool keeps the summary and coverage and returns deterministic prefixes of whole findings and slices. Narrow the range or model to request omitted detail; the scan continuation does not page through findings.

The audit never reads event payloads or invokes modeled commands. It discloses type IDs, metadata-derived counts and positions, recognized producer identities, and consumer subscription references. Event metadata does not supply the stored generation, so the tool uses the latest registered generation, or 1 for unregistered types. It does not expose source or stream IDs, identities, raw causation, schemas, or upstream exception details. It cannot verify handler correctness, conditional logic, authorization, or read-model state.

Use [`audit_unconsumed_event_types`](unconsumed-event-audit.md) for a registry and subscription inventory without a model. `cratis screenplay conform-namespace` uses the same comparison engine for local models; the MCP tool bounds the model source, scan, transport, and response independently.
