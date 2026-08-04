using UnityEngine;

public class AISystem
{
    public InputValidationAI InputValidator { get; }
    public DetectiveAI Detective { get; }
    public DeductionEvaluationAI Evaluator { get; }

    public AISystem(AIEdgeFunctionClient client)
    {
        InputValidator = new InputValidationAI(client);
        Detective = new DetectiveAI(client);
        Evaluator = new DeductionEvaluationAI(client);
    }
}
