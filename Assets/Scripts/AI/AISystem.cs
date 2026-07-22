using UnityEngine;

public class AISystem
{
    public InputValidationAI InputValidator { get; }
    public DetectiveAI Detective { get; }
    public DeductionEvaluationAI Evaluator { get; }

    public AISystem(
        OpenAIClient client,
        string inputValidationInstruction,
        string detectiveInstruction,
        string evaluationInstruction)
    {
        InputValidator =
            new InputValidationAI(client, inputValidationInstruction);

        Detective =
            new DetectiveAI(client, detectiveInstruction);

        Evaluator =
            new DeductionEvaluationAI(client, evaluationInstruction);
    }
}
