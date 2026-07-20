using UnityEngine;

public class AISystem
{
    public InputValidationAI InputValidator { get; }
    public DetectiveAI Detective { get; }
    public DeductionEvaluationAI Evaluator { get; }

    public AISystem(
        OpenAIClient client,
        string model,
        string inputValidationInstruction,
        string detectiveInstruction,
        string evaluationInstruction)
    {
        InputValidator =
            new InputValidationAI(client, model, inputValidationInstruction);

        Detective =
            new DetectiveAI(client, model, detectiveInstruction);

        Evaluator =
            new DeductionEvaluationAI(client, model, evaluationInstruction);
    }
}
