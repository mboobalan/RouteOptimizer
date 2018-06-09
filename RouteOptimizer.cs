using Google.OrTools.ConstraintSolver;
using RouteOptimizer.Models;
using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;

namespace RouteOptimizer.Controllers
{
 
    public class ValuesController : ApiController
    {

         double[,] distanceArray;
      
      
       
        // POST api/values
        [HttpPost]
        public IList<string> GetShortestOptimizedRoute([FromBody]GeographicViewModel  geographicVM)
        {
          // double[] latitude = { 13.121329, 13.065150, 13.024346, 13.027691, 12.913887, 12.915754, 12.962431, 12.890461, 12.907220, 12.954234, 13.026996, 13.041044, 13.001573 };
            //double[] longitude = { 80.029049, 80.128613, 79.909573, 80.259762, 80.253067, 80.192041, 80.253839, 80.097198, 80.142088, 80.188437, 80.107756, 80.234957, 80.257616 };
            string[] city_names = { "Thirunindravur", "Thiruverkadu", "Senkadu", "Milapore", "VGP Golden", "Medavakkam", "Palavakkam", "Vandalur", "Selaiyur", "Kelkattalai", "Mangadu", "TNagar", "Adyar" };

            double[] latitudeandLongitude;
            var sCoordlatitudeandLongitude = new List<string>();
            var finallatitudeandLongitude = new List<string>();
            distanceArray = new double[geographicVM.Latitude.Length, geographicVM.Longitude.Length];
            var k = 0;
            for (int i = 0; i < geographicVM.Latitude.Length; i++)
            {
                for (int j = 0; j < geographicVM.Longitude.Length; j++)
                {
                    var sCoord = new GeoCoordinate(geographicVM.Latitude[i], geographicVM.Longitude[i]);
                    var eCoord = new GeoCoordinate(geographicVM.Latitude[j], geographicVM.Longitude[j]);
                    if (i == j )
                    {
                       
                        if (i % 2 == 0)
                        {
                            sCoordlatitudeandLongitude.Add(sCoord.ToString()+ ",D" + ","+ k++);
                        }
                        else
                        {
                            sCoordlatitudeandLongitude.Add(sCoord.ToString() + ",P" + "," +k);
                        }
                       
                    }
                    
                    var text = sCoord.GetDistanceTo(eCoord) / 1609.344;
                    distanceArray[i, j] = Math.Round(text);

                }

            }
            double[,] costs = distanceArray;
          
            int num_locations = city_names.Length;
            RoutingModel routingModel = new RoutingModel(geographicVM.Latitude.Length, 1, 0);

            Solver solver = routingModel.solver();


            string rank_name = "rank";
           routingModel.AddConstantDimension(1, geographicVM.Latitude.Length, true, rank_name);
            var rank_dimension = routingModel.GetDimensionOrDie(rank_name);

            //Constraint MinneapolisBeforeNewYork = solver.MakeLess(routingModel.CumulVar(highPriorityVarIndex, rank_name), routingModel.CumulVar(lowPriorityVarIndex, rank_name));
           
            /* Later needs to be worked on the Constraint for the  Multiple Pickups before Delivery */
            //Constraint test = routingModel.CumulVar(3, rank_name) < routingModel.CumulVar(13, rank_name);
           //solver.Add(test);


            RoutingSearchParameters search_parameters = RoutingModel.DefaultSearchParameters();
            search_parameters.FirstSolutionStrategy =
                 FirstSolutionStrategy.Types.Value.LocalCheapestArc;
         NodeEvaluator2 cost_between_nodes = new Cost(costs);
          routingModel.SetArcCostEvaluatorOfAllVehicles(cost_between_nodes); //oder SetVehicleCost wenn Fahrzeuge unterschiedliche Kostenmatrix haben

            StringBuilder route = new StringBuilder();

            Assignment assignment = routingModel.SolveWithParameters(search_parameters);
            if (assignment != null)
            {
                Console.WriteLine("Total distance: " + assignment.ObjectiveValue().ToString() + "miles");

                long index = routingModel.Start(0); //vehicle 0
               
                route.Append("Route: ");

                do
                {
                    //route.Append(city_names[routingModel.IndexToNode(index)] + " -> ");
                    finallatitudeandLongitude.Add(sCoordlatitudeandLongitude[routingModel.IndexToNode(index)]);
                    index = assignment.Value(routingModel.NextVar(index));
                   
                }
                while (!routingModel.IsEnd(index));
               // route.Append(city_names[routingModel.IndexToNode(index)]);
                finallatitudeandLongitude.Add(sCoordlatitudeandLongitude[routingModel.IndexToNode(index)]);
            }
            
           
            return finallatitudeandLongitude;
        }

     
            class Cost : NodeEvaluator2
            {
            public Cost(double[,] costs)
            {
            this.costs_ = costs;
            }

            public override long Run(int first_index, int second_index)
            {
            return (long)costs_[first_index, second_index];
            }

            private double[,] costs_;
            }
                }
}
