# Beagle Python Wrapper



The Beagle Python wrapper (Beagle.py) provides a SKLearn compatible wrapper to use the Beagle framework in a more familiar environment and to allow it to work as a drop in replacement inside of existing SKLearn pipelines.



## Overview



The `BeagleRegressor` class provides a bridge between Python and the Beagle C# implementation. It automates the data exchange (via CSV), generates the necessary C# setup code, executes the Beagle engine using `dotnet run`, and parses the results back into Python.



## Requirements

Python version 3.0 or greater

### Python Dependencies

- `numpy`

- `sympy`

- `pandas`

- `matplotlib`

- `scikit-learn`

- `ipython`





```bash

pip install numpy sympy pandas matplotlib scikit-learn ipython

```



### System Dependencies

- **.NET 10 SDK**: Required to run the Beagle engine via the `dotnet` command.
You can download it here: https://dotnet.microsoft.com/en-us/download/dotnet/10.0

- **Beagle C# Project**: This wrapper expects the Beagle source code to be present in the same directory. If you download the project from GitHub, it will already exist in the correct location. If not, place it inside the beagle-v1.x folder. To use the Beagle Python wrapper you should be running Python from the same directory where Beagle.py file is located.



## Quick Start Tutorial



Here is a simple example of how to use the `BeagleRegressor` to find a symbolic expression for a simple function.



```python

from Beagle import *
 
 
 
# 1. Prepare your data
 
# X should be a 2D array (n_samples, n_features)
 
# y should be a 1D array (n_samples)
 
X = np.random.rand(100,2)
 
y =   X[:, 0]** 2 + 3 * X[:, 1] + 2  # y = x0^2 + 3*x1 + 2
 
 
 
# 2. Initialize the BeagleRegressor
 
# You can tune parameters to control the search process
 
model = BeagleRegressor(
 
    stop_after_min=2,      # Total number of evaluations allowed
 
    pop_size=250000,               # Target population size
 
    fitness_func="Corr",          # 'Corr' for correlation, 'Pt' for other metrics
 
    terminate_on_solution=False   # Continue searching even if a good solution is found
 
)
 
 
 
# 3. Fit the model
 
# Setting live_tracking=True will display a real-time matplotlib plot
 
# of the best fitness found per generation.
 
model.fit(X, y, live_tracking=True)
 
#If running in terminal, it will keep printing Figure(640x480) for the 2 minute run we requested.
#The Figure(640x480) represents the fitness curve. If you wanted to actually see the fitness curve figure, run this in a Jupyter notebook 
 
# 4. Make predictions
 
X_test = np.array([[0.0, 2.0], [1.0, 3.0], [-1.0, 1.5]])
 
predictions = model.predict(X_test)
 
print(f"Predictions: {predictions}")

#The expected results should be: Predictions: [ 8.  12.   7.5]
 
# 5. Inspect the discovered symbolic expression
 
# This returns the model as a symbolic expression
 
expression = model.modelExpression()
 
print(f"Best Symbolic Expression: {expression[0]}")

#Expected expression: Best Symbolic Expression: 1.0*x0**2 + 3.0*x1 + 2.0

```



## API Reference
 
 
 
Below is an overview of what features are available in the Python wrapper.
 
 
 
### `BeagleRegressor`
 
 
 
The object representation for the symbolic regression engine.
 
 
 
#### `__init__(...)`
 
Initializes the regressor with various search constraints.
 
 
 
| Parameter | Type | Default | Description |
 
| :--- | :--- | :--- | :--- |
 
| `evaluation_limit` | `int` | `None` | Maximum number of evaluations before stopping |
 
| `pop_size` | `int` or `[int, ...]` | `None` | The target population size or a list of sizes to switch through at generation counts defined by pop_size_strategy |

| `pop_size_strategy` | `[int, ...]` | `None` | If using multiple population sizes, sets the generation thresholds to switch to the next defined size |

| `pop_size_mod` | `int` | `None` | Number of generations that the population size will restart the cycle (if multiple sizes were set). Defined as $generations \% pop\_size\_mod$| 
 
| `experiments_per_generation` | `int` | `512` | Number of experiments to run in each generation |
 
| `evaluations_to_colony_reset` | `int` | `None` | Number of total births before resetting the colony if no progress is made |
 
| `run_feynman` | `int` | `None` | If set, enables Feynman benchmark search mode |
 
| `stop_after_min` | `int` | `None` | Maximum runtime in minutes |
 
| `solution_threshold` | `float` | `None` | A fitness threshold (0.0 to 1.0] that, if reached, signals a found solution |
 
| `terminate_on_solution` | `bool` | `False` | If `True`, stops immediately once `solution_threshold` is met |
 
| `fitness_func` | `str` | `"Corr"` | The fitness function to use: `"Corr"` (Correlation) or `"Pt"` |

| `variable_names` | `[str,...]` | `None` | A list of variable names for the model to use when displaying the symbolic expression. 
 
 
 
#### `fit(X, y, show_training_curve=False, live_tracking=False)`
 
Executes the symbolic regression search.
 
 
 
- `X`: Training input data (`n_samples`, `n_features`).
 
- `y`: Training target data (`n_samples`).
 
- `show_training_curve`: If `True`, displays a static plot of the training curve after fitting.
 
- `live_tracking`: If `True`, provides a real-time updating plot of the best fitness per generation during the search.
 
 
 
#### `predict(X)`
 
Returns the predicted values for the input `X` using the best discovered expression.
 
 
 
#### `modelExpression()`
 
Returns the symbolic representation of the best-found model as a list of `sympy` objects.
 
 
 
### `BeagleModel`
 
 
 
A representation of a single model from Beagle. Can be used to explore a model in the Python environment that was developed outside of the wrapper by directly using Beagle in C#.
 
 
 
####`__init__(expression)`
 
 
 
Create a Python object for a given expression.
 
 
 
#### `evalute(X)`
 
Evaluates the model using input data `X`
 
 
 
#### `modelExpression()`
 
Prints the symbolic expression.