using BeagleLib.Engine;
using BeagleLib.Engine.FitFunc;

namespace Run.Feynman100;

public static class FeynmanBenchmark
{
    public static MLEngineCore GetFeynmanMLEngineForFormula<TFitFunc>(int formulaId) where TFitFunc : struct, IFitFunc
    {
        switch (formulaId)
        {
            case 1: return new MLEngine<Eq1, TFitFunc>(forceCPUAccelerator: false);
            case 2: return new MLEngine<Eq2, TFitFunc>(forceCPUAccelerator: false);
            case 3: return new MLEngine<Eq3, TFitFunc>(forceCPUAccelerator: false);
            case 4: return new MLEngine<Eq4, TFitFunc>(forceCPUAccelerator: false);
            case 5: return new MLEngine<Eq5, TFitFunc>(forceCPUAccelerator: false);

            case 6: return new MLEngine<Eq6, TFitFunc>(forceCPUAccelerator: false);
            case 7: return new MLEngine<Eq7, TFitFunc>(forceCPUAccelerator: false);
            case 8: return new MLEngine<Eq8, TFitFunc>(forceCPUAccelerator: false);
            case 9: return new MLEngine<Eq9, TFitFunc>(forceCPUAccelerator: false);
            case 10: return new MLEngine<Eq10, TFitFunc>(forceCPUAccelerator: false);
            case 11: return new MLEngine<Eq11, TFitFunc>(forceCPUAccelerator: false);
            case 12: return new MLEngine<Eq12, TFitFunc>(forceCPUAccelerator: false);
            case 13: return new MLEngine<Eq13, TFitFunc>(forceCPUAccelerator: false);
            case 14: return new MLEngine<Eq14, TFitFunc>(forceCPUAccelerator: false);
            case 15: return new MLEngine<Eq15, TFitFunc>(forceCPUAccelerator: false);
            case 16: return new MLEngine<Eq16, TFitFunc>(forceCPUAccelerator: false);
            case 17: return new MLEngine<Eq17, TFitFunc>(forceCPUAccelerator: false);

            case 18: return new MLEngine<Eq18, TFitFunc>(forceCPUAccelerator: false);
            case 19: return new MLEngine<Eq19, TFitFunc>(forceCPUAccelerator: false);
            case 20: return new MLEngine<Eq20, TFitFunc>(forceCPUAccelerator: false);
            case 21: return new MLEngine<Eq21, TFitFunc>(forceCPUAccelerator: false);
            case 22: return new MLEngine<Eq22, TFitFunc>(forceCPUAccelerator: false);
            case 23: return new MLEngine<Eq23, TFitFunc>(forceCPUAccelerator: false);
            case 24: return new MLEngine<Eq24, TFitFunc>(forceCPUAccelerator: false);
            case 25: return new MLEngine<Eq25, TFitFunc>(forceCPUAccelerator: false);

            case 26: return new MLEngine<Eq26, TFitFunc>(forceCPUAccelerator: false);
            case 27: return new MLEngine<Eq27, TFitFunc>(forceCPUAccelerator: false);
            case 28: return new MLEngine<Eq28, TFitFunc>(forceCPUAccelerator: false);
            case 29: return new MLEngine<Eq29, TFitFunc>(forceCPUAccelerator: false);
            case 30: return new MLEngine<Eq30, TFitFunc>(forceCPUAccelerator: false);
            case 31: return new MLEngine<Eq31, TFitFunc>(forceCPUAccelerator: false);
            case 32: return new MLEngine<Eq32, TFitFunc>(forceCPUAccelerator: false);
            case 33: return new MLEngine<Eq33, TFitFunc>(forceCPUAccelerator: false);
            case 34: return new MLEngine<Eq34, TFitFunc>(forceCPUAccelerator: false);
            case 35: return new MLEngine<Eq35, TFitFunc>(forceCPUAccelerator: false);
            case 36: return new MLEngine<Eq36, TFitFunc>(forceCPUAccelerator: false);
            case 37: return new MLEngine<Eq37, TFitFunc>(forceCPUAccelerator: false);
            case 38: return new MLEngine<Eq38, TFitFunc>(forceCPUAccelerator: false);
            case 39: return new MLEngine<Eq39, TFitFunc>(forceCPUAccelerator: false);
            case 40: return new MLEngine<Eq40, TFitFunc>(forceCPUAccelerator: false);
            case 41: return new MLEngine<Eq41, TFitFunc>(forceCPUAccelerator: false);
            case 42: return new MLEngine<Eq42, TFitFunc>(forceCPUAccelerator: false);
            case 43: return new MLEngine<Eq43, TFitFunc>(forceCPUAccelerator: false);
            case 44: return new MLEngine<Eq44, TFitFunc>(forceCPUAccelerator: false);
            case 45: return new MLEngine<Eq45, TFitFunc>(forceCPUAccelerator: false);
            case 46: return new MLEngine<Eq46, TFitFunc>(forceCPUAccelerator: false);
            case 47: return new MLEngine<Eq47, TFitFunc>(forceCPUAccelerator: false);
            case 48: return new MLEngine<Eq48, TFitFunc>(forceCPUAccelerator: false);
            case 49: return new MLEngine<Eq49, TFitFunc>(forceCPUAccelerator: false);
            case 50: return new MLEngine<Eq50, TFitFunc>(forceCPUAccelerator: false);
            case 51: return new MLEngine<Eq51, TFitFunc>(forceCPUAccelerator: false);
            case 52: return new MLEngine<Eq52, TFitFunc>(forceCPUAccelerator: false);
            case 53: return new MLEngine<Eq53, TFitFunc>(forceCPUAccelerator: false);
            case 54: return new MLEngine<Eq54, TFitFunc>(forceCPUAccelerator: false);
            case 55: return new MLEngine<Eq55, TFitFunc>(forceCPUAccelerator: false);
            case 56: return new MLEngine<Eq56, TFitFunc>(forceCPUAccelerator: false);
            case 57: return new MLEngine<Eq57, TFitFunc>(forceCPUAccelerator: false);
            case 58: return new MLEngine<Eq58, TFitFunc>(forceCPUAccelerator: false);
            case 59: return new MLEngine<Eq59, TFitFunc>(forceCPUAccelerator: false);
            case 60: return new MLEngine<Eq60, TFitFunc>(forceCPUAccelerator: false);
            case 61: return new MLEngine<Eq61, TFitFunc>(forceCPUAccelerator: false);
            case 62: return new MLEngine<Eq62, TFitFunc>(forceCPUAccelerator: false);
            case 63: return new MLEngine<Eq63, TFitFunc>(forceCPUAccelerator: false);
            case 64: return new MLEngine<Eq64, TFitFunc>(forceCPUAccelerator: false);
            case 65: return new MLEngine<Eq65, TFitFunc>(forceCPUAccelerator: false);
            case 66: return new MLEngine<Eq66, TFitFunc>(forceCPUAccelerator: false);
            case 67: return new MLEngine<Eq67, TFitFunc>(forceCPUAccelerator: false);
            case 68: return new MLEngine<Eq68, TFitFunc>(forceCPUAccelerator: false);
            case 69: return new MLEngine<Eq69, TFitFunc>(forceCPUAccelerator: false);
            case 70: return new MLEngine<Eq70, TFitFunc>(forceCPUAccelerator: false);
            case 71: return new MLEngine<Eq71, TFitFunc>(forceCPUAccelerator: false);
            case 72: return new MLEngine<Eq72, TFitFunc>(forceCPUAccelerator: false);
            case 73: return new MLEngine<Eq73, TFitFunc>(forceCPUAccelerator: false);
            case 74: return new MLEngine<Eq74, TFitFunc>(forceCPUAccelerator: false);
            case 75: return new MLEngine<Eq75, TFitFunc>(forceCPUAccelerator: false);
            case 76: return new MLEngine<Eq76, TFitFunc>(forceCPUAccelerator: false);
            case 77: return new MLEngine<Eq77, TFitFunc>(forceCPUAccelerator: false);
            case 78: return new MLEngine<Eq78, TFitFunc>(forceCPUAccelerator: false);
            case 79: return new MLEngine<Eq79, TFitFunc>(forceCPUAccelerator: false);
            case 80: return new MLEngine<Eq80, TFitFunc>(forceCPUAccelerator: false);
            case 81: return new MLEngine<Eq81, TFitFunc>(forceCPUAccelerator: false);
            case 82: return new MLEngine<Eq82, TFitFunc>(forceCPUAccelerator: false);
            case 83: return new MLEngine<Eq83, TFitFunc>(forceCPUAccelerator: false);
            case 84: return new MLEngine<Eq84, TFitFunc>(forceCPUAccelerator: false);
            case 85: return new MLEngine<Eq85, TFitFunc>(forceCPUAccelerator: false);
            case 86: return new MLEngine<Eq86, TFitFunc>(forceCPUAccelerator: false);
            case 87: return new MLEngine<Eq87, TFitFunc>(forceCPUAccelerator: false);
            case 88: return new MLEngine<Eq88, TFitFunc>(forceCPUAccelerator: false);
            case 89: return new MLEngine<Eq89, TFitFunc>(forceCPUAccelerator: false);
            case 90: return new MLEngine<Eq90, TFitFunc>(forceCPUAccelerator: false);
            case 91: return new MLEngine<Eq91, TFitFunc>(forceCPUAccelerator: false);
            case 92: return new MLEngine<Eq92, TFitFunc>(forceCPUAccelerator: false);
            case 93: return new MLEngine<Eq93, TFitFunc>(forceCPUAccelerator: false);
            case 94: return new MLEngine<Eq94, TFitFunc>(forceCPUAccelerator: false);
            case 95: return new MLEngine<Eq95, TFitFunc>(forceCPUAccelerator: false);
            case 96: return new MLEngine<Eq96, TFitFunc>(forceCPUAccelerator: false);
            case 97: return new MLEngine<Eq97, TFitFunc>(forceCPUAccelerator: false);
            case 98: return new MLEngine<Eq98, TFitFunc>(forceCPUAccelerator: false);
            case 99: return new MLEngine<Eq99, TFitFunc>(forceCPUAccelerator: false);
            case 100: return new MLEngine<Eq100, TFitFunc>(forceCPUAccelerator: false);

            default: throw new Exception($"Invalid Feynman Equation number {formulaId}. Mist be 1-100.");
        }


    }
}