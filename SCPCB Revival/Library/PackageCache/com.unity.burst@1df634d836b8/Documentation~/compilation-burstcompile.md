# Marking code for Burst compilation

Apply the [`[BurstCompile]`](xref:Unity.Burst.BurstCompileAttribute) attribute to the parts of your code you want Burst to compile. You can apply the `[BurstCompile]` attribute to the following:

* **Jobs**: When you apply `[BurstCompile]` to a job definition, Burst compiles everything within the job. For more information on jobs, refer to [Job system](xref:um-job-system) in the Unity Manual.
* **Classes**: Apply `[BurstCompile]` to a class definition if the class contains static methods that are also marked with `[BurstCompile]`. Burst can't compile the class itself but only its member methods.
* **Structs**: Apply `[BurstCompile]` to a regular (non-job) struct definition if the struct contains static methods that are also marked with `[BurstCompile]`.
* **Static methods**: Apply `[BurstCompile]` to the method and its parent type. To work with dynamic functions that process data based on other data states, refer to [Function pointers](csharp-function-pointers.md).
* **Assemblies**: Apply `[BurstCompile]` to an assembly to set options for all Burst jobs and function-pointers within the assembly. For more information, refer to [Defining Burst options for an assembly](compilation-burstcompile-assembly.md).

> [!IMPORTANT]
>You don't always need to mark a method with the `[BurstCompile]` attribute for Burst to compile it. Any method where the program execution switches from managed to Burst-compiled code is referred to as a Burst entry point. If a static entry point method is marked with `[BurstCompile]`, Burst also compiles any methods it calls into, even if they're not marked `[BurstCompile]`.

## Configure Burst compilation with parameters

You can supply parameters to the [`[BurstCompile]`](xref:Unity.Burst.BurstCompileAttribute) attribute to modify aspects of compilation and improve Burst's performance. You can use attribute parameters to: 

* Use a different accuracy for math functions (for example, sin, cos).
* Relax the order of math computations so that Burst can rearrange the floating point calculations.
* Force a synchronous compilation of a job (only for [just-in-time compilation](compilation.md)).

For example, you can use the `[BurstCompile]` attribute to change the [floating precision](xref:Unity.Burst.FloatPrecision) and [float mode](xref:Unity.Burst.FloatMode) of Burst like so: 

    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]

## FloatPrecision

Use the [`FloatPrecision`](xref:Unity.Burst.FloatPrecision) enumeration to define Burst's floating precision accuracy.

Float precision is measured in ulp (unit in the last place or unit of least precision). This is the space between floating-point numbers: the value the least significant digit represents if it's 1. `Unity.Burst.FloatPrecision` provides the following accuracy: 

* `FloatPrecision.Standard`: Default value, which is the same as `FloatPrecision.Medium`. This provides an accuracy of 3.5 ulp. 
* `FloatPrecision.High`: Provides an accuracy of 1.0 ulp.
* `FloatPrecision.Medium`: Provides an accuracy of 3.5 ulp.
* `FloatPrecision.Low`: Has an accuracy defined per function, and functions might specify a restricted range of valid inputs.

**Note:** In previous versions of the Burst API, the `FloatPrecision` enum was named `Accuracy`.

### FloatPrecision.Low

If you use the [`FloatPrecision.Low`](xref:Unity.Burst.FloatPrecision) mode, the following functions have a precision of 350.0 ulp. All other functions inherit the ulp from `FloatPrecision.Medium`.

* `Unity.Mathematics.math.sin(x)`
* `Unity.Mathematics.math.cos(x)`
* `Unity.Mathematics.math.exp(x)`
* `Unity.Mathematics.math.exp2(x)`	
* `Unity.Mathematics.math.exp10(x)`	
* `Unity.Mathematics.math.log(x)`
* `Unity.Mathematics.math.log2(x)`
* `Unity.Mathematics.math.log10(x)`	
* `Unity.Mathematics.math.pow(x, y)`
    * Negative `x` to the power of a fractional `y` aren't supported.
* `Unity.Mathematics.math.fmod(x, y)`

## FloatMode

Use the [`FloatMode`](xref:Unity.Burst.FloatMode) enumeration to define Burst's floating point math mode. It provides the following modes:


* `FloatMode.Default`: Defaults to `FloatMode.Strict` mode.
* `FloatMode.Strict`: Burst doesn't perform any re-arrangement of the calculation and respects special floating point values (such as denormals, NaN). This is the default value.
* `FloatMode.Fast`: Burst can perform instruction re-arrangement and use dedicated or less precise hardware SIMD instructions.
* `FloatMode.Deterministic`: Ensure that floating point calculation in Burst are deterministic, i.e., consistent across all supported platforms. Only supported on 64-bit architectures.

For hardware that can support Multiply and Add (e.g mad `a * b + c`) into a single instruction, you can use `FloatMode.Fast` to enable this optimization. However, the reordering of these instructions might lead to a lower accuracy.

Use `FloatMode.Fast` for scenarios where the exact order of the calculation and the uniform handling of NaN values aren't required.

Use `FloatMode.Deterministic` when your application requires floating point calculations to produce consistent results across different platforms and hardware. 

## More about `FloatMode.Deterministic`

Consistent floating point results are particularly important for deterministic simulations, networking, or any scenario where cross-platform reproducibility is critical.

When `FloatMode.Deterministic` is enabled:
* Burst will use deterministic implementations of math functions.
* Burst will disable certain floating-point optimizations that can introduce platform-specific differences.
* Subnormal (denormal) floating-point numbers are flushed to zero on all platforms, eliminating inconsistencies due to subnormal handling.

Disabling some optimizations may negatively impact the performance of floating point calculations. The actual performance impact will depend on your specific use case.

`NaN` values in IEEE 754 are encoded with all exponent bits set to one and a non-zero significand. `FloatMode.Deterministic` does not guarantee bitwise-identical `NaN` (Not a Number) representations. If a calculation results in a `NaN`, all platforms will produce a `NaN`, but the exact bit pattern of the `NaN` may vary across platforms.

Use of hardware-specific intrinsics or platform-specific instructions can still cause differences across platforms and may break determinism if not avoided.

Keep in mind that `FloatMode.Deterministic` only applies to Burst-compiled code. Determinism can still be compromised by several factors, such as:

* If the input to Burst-compiled code includes floating-point calculations, this may introduce non-determinism.
* If you pass a function pointer from code that is not set to use FloatMode.Deterministic into Burst-compiled code that is, determinism may be broken.


## Additional resources

* [`[BurstCompile]` attribute API reference](xref:Unity.Burst.BurstCompileAttribute)
* [Defining Burst options for an assembly](compilation-burstcompile-assembly.md)

