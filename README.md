# Smart Farmer

Smart Farmer is a test of social improvement of home-made farming. The goal is to share a framework where passionate people can control in automatic and sustainable way their own garden

# Practical information

The Smart Farmer framework organizes the domain in three main concepts:
- **Ground**: where the farmer plays.
- **Plant**: it is the family of known plants.
- **Plan**: is a sequence of Actions, or **tasks**, that the controller executes to carry the ground on. 
- **Alerts**: is a message that tasks may raise, if notifiable information happen. 

## Ground

The ground is the playground. It is organized as a grid with many plants on it. Each plant has coordinates and an occupancy. The ground has a collection of plans, as well, that are executed to play with the ground itself.

The ground has a special task, as well, the _Auto irrigation task_, that runs the irrigation automatically, where applicable. 

## Plants

Smart Farmer has a set of known plants. When placed on a ground, they become Plant instance; each plant in the ground has a given name

## Plans & Tasks

is a sequence of Actions, or **tasks**, that the controller executes to carry the ground on. 

Example of tasks:
- seed
- provide water
- fertilize
- remove weed
- check plant status

All of them requires general information about the agriculture and knowledge on the plant they are acting on.

Running tasks may raise notifiable information or issues to the farmer. This information are **alerts**, with codes, messages, levels of urgency and impact
