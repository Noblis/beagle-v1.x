\# Beagle Python Wrapper



The Beagle Python wrapper (Beagle.py) provides a SKLearn compatible wrapper to use the Beagle framework in a more familiar environment and to allow it to work as a drop in replacement inside of existing SKLearn pipelines.



\## Overview



The `BeagleRegressor` class provides a bridge between Python and the Beagle C# implementation. It automates the data exchange (via CSV), generates the necessary C# setup code, executes the Beagle engine using `dotnet run`, and parses the results back into Python.



\## Requirements



\### Python Dependencies

\- `numpy`

\- `sympy`

\- `pandas`

\- `matplotlib`

\- `scikit-learn`

\- `subprocess`

\- `re`

\- `ipython`





```bash

pip install numpy sympy pandas matplotlib scikit-learn subprocess re ipython

```



\### System Dependencies

\- \*\*.NET SDK\*\*: Required to run the Beagle engine via the `dotnet` command.

\- \*\*Beagle C# Project\*\*: This wrapper expects the Beagle source code to be present in the same directory. If you download the project from GitHub, it will already exist in the correct location. If not, place it inside the beagle-v1.x folder. 



\## Quick Start Tutorial



Here is a simple example of how to use the `BeagleRegressor` to find a symbolic expression for a simple function.



```python

from Beagle import \*



\# 1. Prepare your data

\# X should be a 2D array (n\_samples, n\_features)

\# y should be a 1D array (n\_samples)

X = np.linspace(-5, 5, 100).reshape(-1, 2)

y =   X\[:, 0] \*\* 2 + 3 \* X\[:, 1] + 2  # y = x0^2 + 3\*x1 + 2



\# 2. Initialize the BeagleRegressor

\# You can tune parameters to control the search process

model = BeagleRegressor(

&#x20;   stop\_after\_min=2,      # Total number of evaluations allowed

&#x20;   pop\_size=250000,               # Target population size

&#x20;   fitness\_func="Corr",          # 'Corr' for correlation, 'Pt' for other metrics

&#x20;   terminate\_on\_solution=False   # Continue searching even if a good solution is found

)



\# 3. Fit the model

\# Setting live\_tracking=True will display a real-time matplotlib plot 

\# of the best fitness found per generation.

model.fit(X, y, live\_tracking=True)



\# 4. Make predictions

X\_test = np.array(\[\[0.0, 2.0], \[1.0, 3.0], \[-1.0, 1.5]])

predictions = model.predict(X\_test)

print(f"Predictions: {predictions}")



\# 5. Inspect the discovered symbolic expression

\# This returns the model as a symbolic expression

expression = model.modelExpression()

print(f"Best Symbolic Expression: {expression\[0]}")

```



\## API Reference



Below is an overview of what features are available in the Python wrapper. 



\### `BeagleRegressor`



The object representation for the symbolic regression engine. 



\#### `\_\_init\_\_(...)`

Initializes the regressor with various search constraints.



| Parameter | Type | Default | Description |

| :--- | :--- | :--- | :--- |

| `evaluation\_limit` | `int` | `None` | Maximum number of evaluations before stopping |

| `pop\_size` | `int` | `None` | The target population size |

| `experiments\_per\_generation` | `int` | `512` | Number of experiments to run in each generation |

| `evaluations\_to\_colony\_reset` | `int` | `None` | Number of total births before resetting the colony if no progress is made |

| `run\_feynman` | `int` | `None` | If set, enables Feynman benchmark search mode |

| `stop\_after\_min` | `int` | `None` | Maximum runtime in minutes |

| `solution\_threshold` | `float` | `None` | A fitness threshold (0.0 to 1.0] that, if reached, signals a found solution |

| `terminate\_on\_solution` | `bool` | `False` | If `True`, stops immediately once `solution\_threshold` is met |

| `fitness\_func` | `str` | `"Corr"` | The fitness function to use: `"Corr"` (Correlation) or `"Pt"` |



\#### `fit(X, y, show\_training\_curve=False, live\_tracking=False)`

Executes the symbolic regression search.



\- `X`: Training input data (`n\_samples`, `n\_features`).

\- `y`: Training target data (`n\_samples`).

\- `show\_training\_curve`: If `True`, displays a static plot of the training curve after fitting.

\- `live\_tracking`: If `True`, provides a real-time updating plot of the best fitness per generation during the search.



\#### `predict(X)`

Returns the predicted values for the input `X` using the best discovered expression.



\#### `modelExpression()`

Returns the symbolic representation of the best-found model as a list of `sympy` objects.



\### `BeagleModel`



A representation of a single model from Beagle. Can be used to explore a model in the Python environment that was developed outside of the wrapper by directly using Beagle in C#. 



\####`\_\_init\_\_(expression)`



Create a Python object for a given expression. 



\#### `evalute(X)`

Evaluates the model using input data `X`



\#### `modelExpression(n\_features)`

Prints the symbolic expression provided the number of total input features available. 





