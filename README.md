# ML-cars

ML-cars is a 3D Unity project focused on machine learning, showcasing how cars can learn to drive optimally between checkpoints using Unity ML-Agents. The project utilizes CodeMonkey's tutorial for setting up ML-Agents and incorporates the PROMETEO asset pack for car controllers and car models. Sound effects were sourced from freesounds.com.

## Features

- Training of cars to navigate between checkpoints using machine learning.
- Utilizes Unity ML-Agents framework for reinforcement learning.
- Uses the PROMETEO asset pack for car controller and model.
- Includes sound effects from freesounds.com.

## Installation

To run this project locally, follow these steps:

1. Clone the repository:

   ```
   git clone https://github.com/yourusername/ML-cars.git
   cd ML-cars
   ```

## Roadmap

While the project is functional, there are several improvements planned, including:

- Enhancing the visual aspects of the project.
- Implementing a random track generation feature.

## Acknowledgments
- CodeMonkey for the helpful ML-Agents tutorial.
- PROMETEO asset pack for providing car controller and model assets.
- freesounds.com for supplying sound effects.

## Current AI's
- FirstUseful: First AI capable of finishing a lap (I think)
- CarAgent: Decent agent that trained on low grip
- CarAgent1: CarAgent after some more training
- AdjustedCar1: Good agent trained off original input (didn't have distance to next next waypoint)
-  New model trained knowing distance between target waypoint the waypoint after it
