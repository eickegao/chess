#ifndef SEARCH_H
#define SEARCH_H

using namespace std;

#include <string>
#include "Board.hpp"

/**
 * @class Search
 * @brief Searches the game tree for the best move
 *
 */

class Search
{
    public:
        /**
		 * @fn RootAlphaBeta
		 * @brief Performs an Alpha-Beta search starting at the root node
		 * @param gameBoard The game to search
		 * @param playerColor An int representing the color of the player to make a move for. 1 for white, -1 for black
		 * @param remainingDepth The depth to search to
		 * @return string String representing the best move
		 */
        string RootAlphaBeta(Board gameBoard, int playerColor, int remainingDepth);
        /**
		 * @fn AlphaBeta
		 * @brief Searches the game tree to a given depth
		 * @param board The game to search
		 * @param playerColor An int representing the color of the player to make a move for. 1 for white, -1 for black
		 * @param remainingDepth The depth to search to
		 * @return int The score of the position
		 */
        int AlphaBeta(Board board, int alpha, int beta, int remainingDepth, int playerColor);
};

#endif // SEARCH_H