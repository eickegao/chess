#ifndef UCI_H
#define UCI_H

#include <string>
#include "Board.hpp"

using namespace std;

/**
 * @class UCI
 * @brief Handles the UCI communication protocol
 *
 */

class UCI
{
    private:
        Board currentBoard = Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"); /**< The board set-up with the position command */
        int currentColor = 1; /**< The player who is next to move. 1 for white, -1 for black */
    public:
        bool quit = 0; /**< A flag to be set to true when the quit command is received */
        /**
		 * @fn WaitForInput
		 * @brief Waits to receive input via stdin
		 * @return bool Returns true if the command is recognised, false if not
		 */
        bool waitForInput();
        /**
		 * @fn outputBestMove
		 * @brief Outputs the best move
		 * @param moveString A string giving the best move in algebraic notation
		 * @return void
		 */
        void outputBestMove(string moveString); //string like e2e4
        /**
		 * @fn identification
		 * @brief Outputs the name of the engine and the authors
		 * @return void
		 */
        void identification();
        /**
		 * @fn sentPosition
		 * @brief Sets up a position in response to the position UCI command
		 * @param input A string containing the input from stdin
		 * @return bool Returns true if the position can be set up and false if not
		 */
        bool sentPosition(string input);
        /**
		 * @fn startCalculating
		 * @brief Starts searching for the best move at the current position
		 * @param input A string containing the input from stdin
		 * @return bool Returns true if the command is valid and false if not
		 */
        bool startCalculating(string input);
        /**
		 * @fn sendInfo
		 * @brief Used for outputting information during calculation
		 * @param info A string containing the information to output
		 * @return void
		 */
        void sendInfo(string info);
};

#endif // UCI_H