# AI-cars-1
My own implementation of how AI cars would work

Uses a waypoint system but cars still need to find optimum path between waypoints

**To do**
- Improve the visuals
- Develop an AI capable of driving a track on the fly, that can improve with a bit more training specific to that track
- Find a way to develop random racetracks

**Current AI's**
- FirstUseful: First AI capable of finishing a lap (I think)
- CarAgent: Decent agent that trained on low grip
- CarAgent1: CarAgent after some more training
- AdjustedCar1: Good agent trained off original input (didn't have distance to next next waypoint)
-  New model trained knowing distance between target waypoint the waypoint after it
