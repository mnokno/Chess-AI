# Chess-AI
Chess AI powered by a minimax algorithm with alpha beta optimalization, progressive deepening, quiescence search and opening bokk.
Position is represented using a hybrid of a square centric and a bitboards and is progressive updated. 
Legal move generator is used for move generation.

Features:

[^1]: Minimax algorithm with alpha-beta optimization: This AI employs the minimax algorithm with alpha-beta optimization to determine the best move to make based on the current state of the game. This approach helps reduce the search space and results in faster and more efficient decision-making.

[^2]: Progressive deepening: The AI uses progressive deepening to improve its decision-making ability. It starts by searching a limited depth of moves and progressively increases the depth of the search until it finds the optimal move.

[^3]: Hybrid representation of the position: This AI uses a hybrid representation of the position that combines a square-centric approach with bitboards. This approach allows for efficient and fast updates to the game state, which is essential for real-time decision-making.

[^4]: Legal move generator: The AI uses a legal move generator to generate all possible moves based on the current state of the game. This approach ensures that the AI only considers legal moves, which improves the accuracy of its decision-making.

[^5]: Quiescence search: The AI could also be improved by implementing a quiescence search to evaluate positions that are stable and quiet. This approach can help prevent the AI from making decisions based on temporary gains and improve its overall decision-making ability.

[^6]: Opening book: An opening book can be used to provide the AI with pre-calculated moves for the opening phase of the game. This approach can help the AI make faster and more efficient decisions in the early stages of the game.
