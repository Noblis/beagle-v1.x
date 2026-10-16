import numpy as np
from sympy import symbols
import subprocess as sp
import re
import sympy as sym
import pandas as pd
import matplotlib.pyplot as plt
import time
import os
try:
    from IPython.display import display, clear_output
except:
    pass
# ─── SKLearn Type Beagle Wrapper ──────────────────────────────────────────────────────────
class BeagleModel:

    def __init__(self, expression, variableNames=[]):
        self.expression = expression
        self.variableNames = variableNames

    def evaluate(self, X):
        stack = []
        copyPasteDict = {}
        expr = self.expression.split('; ')
        data = np.array(X).T
        for op in expr:
            if "CONST" in op:
                stack.append(float(op.split(' ')[1]))
            if "LOAD" in op:
                stack.append(data[int(op.split(':')[1])])
            if "ADD" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a + b)
            if "SUB" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a - b)
            if "MUL" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a * b)
            if "DIV" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a / b)
            if "POW" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a ** b)
            if "SIGN" in op:
                a = stack.pop()
                stack.append(np.sign(a))
            if "SQUARE" in op:
                a = stack.pop()
                stack.append(a ** 2)
            if "CUBE" in op:
                a = stack.pop()
                stack.append(a ** 3)
            if "SQRT" in op:
                a = stack.pop()
                stack.append(a ** 0.5)
            if "CBRT" in op:
                a = stack.pop()
                stack.append(a ** (1/3))
            if "LN" in op:
                a = stack.pop()
                stack.append(np.log(a))
            if "SIN" in op:
                a = stack.pop()
                stack.append(np.sin(a))
            if "COS" in op:
                a = stack.pop()
                stack.append(np.cos(a))
            if "TAN" in op:
                a = stack.pop()
                stack.append(np.tan(a))
            if "ARCSIN" in op:
                a = stack.pop()
                stack.append(np.arcsin(a))
            if "ARCCOS" in op:
                a = stack.pop()
                stack.append(np.arccos(a))
            if "ARCTAN" in op:
                a = stack.pop()
                stack.append(np.arctan(a))
            if "TANH" in op:
                a = stack.pop()
                stack.append(np.tanh(a))
            if "EXP" in op:
                a = stack.pop()
                stack.append(np.exp(a))
            if "DUP" in op:
                a = stack.pop()
                stack.append(a)
                stack.append(a)
            if "DEL" in op:
                stack.pop()
            if "SWAP" in op:
                a = stack.pop()
                b = stack.pop()
                stack.append(a)
                stack.append(b)
            if "COPY" in op:
                varName = op.split(" ")[1]
                a = stack.pop()
                copyPasteDict[varName] = a
                stack.append(a)
            if "PASTE" in op:
                varName = op.split(" ")[1]
                a = copyPasteDict[varName]
                stack.append(a)
        return stack[0]

    def modelExpression(self, n_features):
        stack = []
        copyPasteDict = {}
        expr = self.expression.split('; ')
        #if len(self.variableNames) == 0:
        
        for i in range(n_features):
            self.variableNames.append(symbols("x" + str(i)))
        #else:
        #    for i in range(len(self.variableNames)):
        #        self.variableNames[i] = symbols(self.variableNames[i])
        
        data = self.variableNames
        
        for op in expr:
            if "CONST" in op:
                stack.append(float(op.split(' ')[1]))
            if "LOAD" in op:
                stack.append(data[int(op.split(':')[1])])
            if "ADD" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a + b)
            if "SUB" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a - b)
            if "MUL" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a * b)
            if "DIV" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a / b)
            if "POW" in op:
                b = stack.pop()
                a = stack.pop()
                stack.append(a ** b)
            if "SIGN" in op:
                a = stack.pop()
                stack.append(sym.sign(a))
            if "SQUARE" in op:
                a = stack.pop()
                stack.append(a ** 2)
            if "CUBE" in op:
                a = stack.pop()
                stack.append(a ** 3)
            if "SQRT" in op:
                a = stack.pop()
                stack.append(a ** 0.5)
            if "CBRT" in op:
                a = stack.pop()
                stack.append(a ** (1/3))
            if "LN" in op:
                a = stack.pop()
                stack.append(sym.log(a))
            if "SIN" in op:
                a = stack.pop()
                stack.append(sym.sin(a))
            if "COS" in op:
                a = stack.pop()
                stack.append(sym.cos(a))
            if "TAN" in op:
                a = stack.pop()
                stack.append(sym.tan(a))
            if "ARCSIN" in op:
                a = stack.pop()
                stack.append(sym.asin(a))
            if "ARCCOS" in op:
                a = stack.pop()
                stack.append(sym.acos(a))
            if "ARCTAN" in op:
                a = stack.pop()
                stack.append(sym.atan(a))
            if "TANH" in op:
                a = stack.pop()
                stack.append(sym.tanh(a))
            if "EXP" in op:
                a = stack.pop()
                stack.append(sym.exp(a))
            if "DUP" in op:
                a = stack.pop()
                stack.append(a)
                stack.append(a)
            if "DEL" in op:
                stack.pop()
            if "SWAP" in op:
                a = stack.pop()
                b = stack.pop()
                stack.append(a)
                stack.append(b)
            if "COPY" in op:
                varName = op.split(" ")[1]
                a = stack.pop()
                copyPasteDict[varName] = a
                stack.append(a)
            if "PASTE" in op:
                varName = op.split(" ")[1]
                a = copyPasteDict[varName]
                stack.append(a)
        return stack
            
class BeagleRegressor:
    """Scikit-learn–compatible wrapper around Beagle's API."""

    def __init__(self, evaluation_limit=300*10**6, pop_size=None, experiments_per_generation=512, evaluations_to_colony_reset=None, run_feynman=None, stop_after_min=None, solution_threshold=None, terminate_on_solution = False, fitness_func = "Corr"):
        self.evaluation_limit = int(evaluation_limit) if evaluation_limit is not None else None
        self.pop_size = int(pop_size) if pop_size is not None else None
        self.experiments_per_generation = int(experiments_per_generation) if experiments_per_generation is not None else None
        self.evaluations_to_colony_reset = int(evaluations_to_colony_reset) if evaluations_to_colony_reset is not None else None
        self.stop_after_min = int(stop_after_min) if stop_after_min is not None else None
        self.run_feynman = int(run_feynman) if run_feynman is not None else None
        self.fitness_func = str(fitness_func) if fitness_func is not None else "Corr"
        self.terminate_on_solution = bool(terminate_on_solution)
        self.solution_threshold = max(min(1.0,float(solution_threshold)),0.0) if solution_threshold is not None else None
        self.n_features_in_ = None
        self.feature_names_ = None
        self.training_curve_ = []
        self.model = None
        if (self.pop_size is not None) and (self.pop_size <= 10000):
            print("Warning: Population size is too small (<=10000) to effectively utilize GPU. This may lead to suboptimal results.")
        if (self.fitness_func not in ["Corr", "Pt"]):
            raise ValueError("Invalid fitness function. Must be either 'Corr' or 'Pt'.")
        if (self.evaluation_limit is None) and (self.time_limit is None):
            raise ValueError("Runtime is unbounded! Either evaluation_limit or time_limit must be a positive integer.")

    def _to_input_list(self, X):
        arr = np.array(X, dtype=float)
        return [arr[:, i] for i in range(arr.shape[1])]

    def fit(self, X, y, show_training_curve=False, live_tracking=False):
        self.training_curve_ = [0]
        self._program_setup()
        self._export_to_csv(X,y)
        self.n_features_in_ = X.shape[1]
        self._generateMLSetup()
        args = ['dotnet', 'run', '--configuration', 'Release', '--project', 'Beagle/Run', "NoEscMenu"]
        if self.evaluation_limit is not None:
            args.append(f"StopAfterBirths={self.evaluation_limit}")
        if self.run_feynman is not None:
            args.append(f"RunFeynman={self.run_feynman}")
        if self.stop_after_min is not None:
            args.append(f"StopAfterMin={self.stop_after_min}")

        if live_tracking:
            ckTime=time.perf_counter()
            fig, ax = plt.subplots()
            ax.set_facecolor("#002B49")
            best_update = False
            result = sp.Popen(
                args,
                stdout=sp.PIPE,
                stderr=sp.PIPE,
                text=True,
                bufsize=1
                    )

            for line in result.stdout:
                asr = None

                if best_update:
                    self.model = BeagleModel(line)
                    best_update = False
                if "Most accurate ever genome" in line:
                    asr, _ = self._extract_asr_and_complexity(line)
                    best_update = True

                if asr is not None:
                    self.training_curve_.append(asr)

                if "Stopped" in line:
                    break
                if time.perf_counter()-ckTime>0.5:
                    ax.clear()
                    ax.grid(True, color='grey', linestyle='--', linewidth=1.2)
                    ax.plot(self.training_curve_, color = "#8DC63F")
                    ax.set_title(f"Best Model: {self.training_curve_[-1]:.2f} at Generation {len(self.training_curve_)-1}")
                    ax.set_xlabel("Generations")
                    ax.set_ylabel("Fitness")
                    clear_output(wait=True) 
                    display(fig)  
                    plt.close(fig)
                    ckTime=time.perf_counter()

            ax.clear()
            ax.grid(True, color='grey', linestyle='--', linewidth=1.2)
            ax.plot(self.training_curve_, color = "#8DC63F")
            ax.set_title(f"Best Model: {self.training_curve_[-1]:.2f} at Generation {len(self.training_curve_)-1}")
            ax.set_xlabel("Generations")
            ax.set_ylabel("Fitness")
            clear_output(wait=True) 
            display(fig)  
            plt.close(fig)
            ckTime=time.perf_counter()

        else: 
            result = sp.run(
                args,  # command and arguments
                capture_output=True,  # capture stdout and stderr
                text=True  # return strings instead of bytes
            )

        #print("STDOUT:\n", result.stdout)
        #print("STDERR:\n", result.stderr)
        #print("Return code:", result.returncode)

            lines = result.stdout.split('\n')
            for i in range(len(lines) - 1, -1, -1):
                if "Most accurate ever genome" in lines[i]:
                    print("(ASR, Complexity)",self._extract_asr_and_complexity(lines[i]))
                    print("Best Model:", lines[i + 1])
                    self.model = BeagleModel(lines[i + 1])
                    break

            for i in range(len(lines)):
                if "Most accurate ever genome" in lines[i]:
                    self.training_curve_.append(self._extract_asr_and_complexity(lines[i])[0])

            if (show_training_curve):
                plt.figure()
                plt.grid(True, color='grey', linestyle='--', linewidth=1.2)
                plt.rcParams["axes.facecolor"] = "#002B49"
                plt.plot(self.training_curve_, color = "#8DC63F")
                plt.ylim(0,1)
                plt.xlabel('Generation')
                plt.ylabel('ASR')
                plt.title('Training Curve')
                plt.show()

        return self

    def predict(self, X):
        if self.model is None:
            raise ValueError("Model not fitted yet.")
        pred = self.model.evaluate(X)
        return np.array(pred, dtype=float)

    def modelExpression(self):
        if self.model is None:
            raise ValueError("Model not fitted yet.")
        return self.model.modelExpression(self.n_features_in_)

    def get_params(self):
        return {
            "evaluation_limit": self.evaluation_limit,
            "pop_size": self.pop_size,
            "time_limit": self.time_limit,
            "experiments_per_generation": self.experiments_per_generation,
            "operator_set": self.operator_set,
            "evaluations_to_colony_reset": self.evaluations_to_colony_reset,
        }

    def _generateMLSetup(self):
        code = """
using BeagleLib.Engine;
using BeagleLib.Util;

namespace Run.MLSetups;

public class PythonMLSetup : MLSetup
{
    private List<float[]> _rows = [];

    public void CsvDataGenerator(string filePath)
    {
        _rows = File.ReadAllLines(filePath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line =>
                line.Split(',')
                    .Select(v => float.Parse(v.Trim()))
                    .ToArray()
            ).ToList();

        if (_rows.Count == 0)
            throw new Exception("CSV file contains no data rows.");
    }
    
    public override (float[], float) GetNextInputsAndCorrectOutput(float[] inputs)
    {
        //Read from csv file
        CsvDataGenerator("PythonData.csv");

        var row = _rows[Rnd.Random.Next(_rows.Count)];

        int inputCount = inputs.Length;

        for (int i = 0; i < inputCount; i++)
        {
            inputs[i] = row[i];
        }

        // Last column is the correct output
        float output = row[inputCount];


        return (inputs, output);
    }
"""
        code += self._set_input_labels()
        code += self._set_population_size(pop_size=self.pop_size)
        code += self._set_evaluations_to_colony_reset(evaluations_to_colony_reset=self.evaluations_to_colony_reset)
        code += self._set_experiments_per_generation(experiments_per_generation=self.experiments_per_generation)
        code += self._set_solution_threshold()
        code += self._set_continue_search_after_solution()
        code += "\n}"

        # export code to .cs file
        with open("Beagle/Run/MLSetups/PythonMLSetup.cs", "w") as f:
            f.write(code)

        return code

    def _set_population_size(self,pop_size=None, strategy=[], mod=None):
        if pop_size is None:
                return ""
        if type(pop_size) == int:
            return f"""
            public override int TargetColonySize(int generations) {{return {pop_size};}}"""
        
        modString="\n\t\tif (generation <"
        if not(mod is None):
            modString = f"\n\t\tif (generation % {mod} <"
        codeChunk = """
        \tpublic override int TargetColonySize(int generation)
        \t{"""
        for i in range(len(strategy)):
            codeChunk += modString + f" {strategy[i]}) return {pop_size[i]};"
        codeChunk += f"\n\t\treturn {pop_size[-1]};\n" + "\t}"
        
        return codeChunk

    def _set_experiments_per_generation(self, experiments_per_generation=None):
        if experiments_per_generation is None:
            return ""
        return f"""
            \t\tpublic override uint ExperimentsPerGeneration => {experiments_per_generation};\n"""

    def _set_evaluations_to_colony_reset(self, evaluations_to_colony_reset=None):
        if evaluations_to_colony_reset is None:
            return ""
        return f"""
            \t\tpublic override long TotalBirthsToResetColonyIfNoProgress => {evaluations_to_colony_reset};\n"""

    def _set_continue_search_after_solution(self):
        if self.terminate_on_solution:
            return """\t\tpublic override bool KeepOptimizingAfterSolutionFound => false;\n"""
        return """\t\tpublic override bool KeepOptimizingAfterSolutionFound => true;\n"""

    def _set_solution_threshold(self):
        if self.solution_threshold is None:
            return "\t\tpublic override double SolutionFoundASRThreshold => 1.0;\n"
        return f"""
            \t\tpublic override double SolutionFoundASRThreshold => {self.solution_threshold};\n"""

    def _set_input_labels(self):
        labels = [f"x{i}" for i in range(0, self.n_features_in_)]
        labels = str(labels).replace("'", "\"")
        return f"""
        public override string[] GetInputLabels()
        {{
        \treturn {labels};
        }}"""
    

    def _extract_asr_and_complexity(self,line):
        # Extract ASR
        #asr_match = re.search(r'ASR = (\d+\.\d+)', line)
        #asr = float(asr_match.group(1)) if asr_match else None

        # Extract (#/#) component to get exact ASR
        match = re.search(r'\(([\d,]+)/([\d,]+)\)', line)

        num = int(match.group(1).replace(",", ""))
        den = int(match.group(2).replace(",", ""))
        asr = num / den

        # Extract complexity
        length_match = re.search(r'length = (\d+)', line)
        if length_match:
            complexity = (int(length_match.group(1)) if length_match else None)
        else:
            complexity = None

        return asr, complexity

    def _export_to_csv(self, X, y):
        if not os.path.exists("AppOutput"):
            os.makedirs("AppOutput")
        fileName = "AppOutput/PythonData.csv"
        df = pd.DataFrame(X)
        df['y'] = y
        df.to_csv(fileName, index=False, header=False)

    def _program_setup(self):
        with open("Beagle/Run/Program.cs", "r") as f:
            content = f.read()

        lines = content.split('\n')

        for i, line in enumerate(lines):
            if " using var mlEngine = new " in line or "\tusing var mlEngine = new " in line:
                if (self.fitness_func == "Corr"):
                    lines[i] = "\t\t\tusing var mlEngine = new MLEngine<PythonMLSetup, CorrelationFitFunc>(useLibDevice: useLibDevice);"
                elif (self.fitness_func == "Pt"):
                    lines[i] = "\t\t\tusing var mlEngine = new MLEngine<PythonMLSetup, StdFitFunc>(useLibDevice: useLibDevice);"
        
        content = '\n'.join(lines)
        
        with open("Beagle/Run/Program.cs", "w") as f:
            f.write(content)
            
