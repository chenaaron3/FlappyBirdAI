# FlappyBirdAI
Unity AI Flappy Bird

Span of Project: 11/28/18

Revisited: March 2019

**Simulation Background**
  This game simulation was inspired by the 2014 mobile hit game "Flappy Bird". In the build, I present a neural network trained within minutes that can successfully dodge thousands of obstacles. The move set in this game is simply to jump or not to jump.

**Where To Play**
https://simmer.io/@apkirito/flappyai

**How To Play**
1. Press Play to play the game yourself.
2. Press Train to train a generation of birds.
3. Press Test to test a trained bird.
4. Use arrow keys to speed up/slow down the simulation.

**Script Accomplishments**
 - used custom Matrix class, Neural Network class from Dino Jump
 - implemented Genetic Algorithm (mutation, cross over, repopulate)
 - trained within minutes to make a super bird
 - HAD A BETTER FITNESS FUNCTION (increased fitness dramatically when bird goes through the middle)
 - created a ui for playing, training, and testing
 - made compatable for Android
 - refactored file system accessing

**Notes**
 - used high mutation (15%)
 - only cross over weights that share the same neuron(rows only)
 - had 2 parents picked from a pool selection algorithm
 - parents made 2 babies instead of 1
 - save best brain as a JSON into a txt file
 - load brain into game
