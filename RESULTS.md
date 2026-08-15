# Beagle births/deaths benchmark — optimization log
Machine: linux, 24 CPUs, NVIDIA GeForce RTX 3080 (10 GB), .NET 10, ILGPU 1.5.3
Harness: QuickBenchmark MLSetup — fixed 1,000,000-organism colony, 512 experiments/generation,
stead-state metrics over gens 2..N. Medians of 3-4 runs per config (stochastic workload, no fixed RNG seed).

Metrics:
- births/s, deaths/s — offspring/deaths throughput (colony-size dependent)
- genAvg, genAccelAvg — mean seconds per generation (wall), mean accelerator seconds per generation
- cells/accelSec — organisms scored per second of GPU time (Σ colony / Σ accel time)
- cells/cpuSec — organisms scored per second of non-GPU time (CPU-side cost per scored cell)

## Final (4 reps x 20 gens)
| metric | baseline | HEAD (A,B,D,E,F,G) | delta |
|---|---|---|---|
| std births/s | 1,266,776 | 1,412,400 | +11.5% |
| std deaths/s | 1,264,769 | 1,427,858 | +12.9% |
| std genAvg | 0.7955s | 0.693s | -12.9% |
| std accelAvg | 0.6245s | 0.5805s | -7.0% |
| std cells/accelSec | 1,610,161 | 1,704,025 | +5.8% |
| std cells/cpuSec | 5,882,239 | 8,923,717 | +51.7% |
| corr births/s | 556,255 | 1,533,618 | +175.7% |
| corr deaths/s | 552,297 | 1,547,220 | +180.2% |
| corr genAvg | 2.3035s | 0.788s | -65.7% |
| corr accelAvg | 2.072s | 0.659s | -68.2% |
| corr cells/accelSec | 612,071 | 1,861,688 | 3.0x |
| corr cells/cpuSec | 5,459,381 | 9,161,812 | +67.8% |

## Patch-by-patch measurements (3 reps x 12 gens unless noted)

| patch | std genAvg (before -> after) | notes |
|---|---|---|
| A persistent streams/buffers/sync | 0.741 -> 0.655 (1 run) / 0.729 (4x20) | small consistent GPU gain |
| B std block reduction | neutral (+-3% noise; exact-score tests added) | identical scores by exact-input test; kept |
| C grid-stride + tunable group size | 0.68 -> 0.92-1.17 | REGRESSION on this workload (one-shot 512-thread block wins) -> REVERTED |
| D correlation reductions | corr genAvg 2.08 -> 0.68-0.79 | corr slightly faster than std now; +3x cells/accelSec |
| E two-phase reproduction (no Interlocked) | births/s +4-5% | removes shared per-child atomics |
| F precomputed valid-opcode table | genAvg -4..-7% | small consistent CPU win |
| G thread-local dead pools | births/s +11%, genAvg 0.715->0.602; cells/cpuSec 6.3M->9.6M | biggest CPU win |

Correctness: BeagleLib.Test kernel-equivalence suite (GPU vs CPU exact on power-of-2 inputs; tolerance on random inputs)
passes for std at block 512 and for correlation. Pre-existing failure: CodeMachineTests.TestTan (float expectation
off by 2 ulp) fails identically on pristine upstream.

git history: f665676 baseline harness -> f74fbee A -> 7d141ac B -> e73c9c2 C ([reverted] 22639b7) -> a88b7cd D
-> 9c38388 E -> 636aed2 F -> 63cbadd G (HEAD)
