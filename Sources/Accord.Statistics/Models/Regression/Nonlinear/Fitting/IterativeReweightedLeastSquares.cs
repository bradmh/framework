﻿// Accord Statistics Library
// The Accord.NET Framework
// http://accord-framework.net
//
// Copyright © César Souza, 2009-2016
// cesarsouza at gmail.com
//
//    This library is free software; you can redistribute it and/or
//    modify it under the terms of the GNU Lesser General Public
//    License as published by the Free Software Foundation; either
//    version 2.1 of the License, or (at your option) any later version.
//
//    This library is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
//    Lesser General Public License for more details.
//
//    You should have received a copy of the GNU Lesser General Public
//    License along with this library; if not, write to the Free Software
//    Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
//

namespace Accord.Statistics.Models.Regression.Fitting
{
    using System;
    using Accord.Math;
    using Accord.Math.Decompositions;
    using Accord.Statistics.Links;
    using Accord.MachineLearning;
    using System.Threading;

    /// <summary>
    ///   Iterative Reweighted Least Squares for Logistic Regression fitting.
    /// </summary>
    /// 
    /// <remarks>
    /// <para>
    ///   The Iterative Reweighted Least Squares is an iterative technique based
    ///   on the Newton-Raphson iterative optimization scheme. The IRLS method uses
    ///   a local quadratic approximation to the log-likelihood function.</para>
    /// <para>
    ///   By applying the Newton-Raphson optimization scheme to the cross-entropy
    ///   error function (defined as the negative logarithm of the likelihood), one
    ///   arises at a weighted formulation for the Hessian matrix. </para>  
    ///   
    /// <para>
    ///   The Iterative Reweighted Least Squares algorithm can also be used to learn
    ///   arbitrary generalized linear models. However, the use of this class to learn
    ///   such models is currently experimental.
    /// </para>
    /// 
    /// <para>
    ///   References:
    ///   <list type="bullet">
    ///     <item><description>
    ///       Bishop, Christopher M.; Pattern Recognition and Machine Learning. 
    ///       Springer; 1st ed. 2006.</description></item>
    ///     <item><description>
    ///       Amos Storkey. (2005). Learning from Data: Learning Logistic Regressors. School of Informatics.
    ///       Available on: http://www.inf.ed.ac.uk/teaching/courses/lfd/lectures/logisticlearn-print.pdf </description></item>
    ///     <item><description>
    ///       Cosma Shalizi. (2009). Logistic Regression and Newton's Method. Available on:
    ///       http://www.stat.cmu.edu/~cshalizi/350/lectures/26/lecture-26.pdf </description></item>
    ///     <item><description>
    ///       Edward F. Conor. Logistic Regression. Website. Available on: 
    ///       http://userwww.sfsu.edu/~efc/classes/biol710/logistic/logisticreg.htm </description></item>
    ///   </list></para>  
    /// </remarks>
    /// 
    /// <example>
    ///   <code>
    ///    // Suppose we have the following data about some patients.
    ///    // The first variable is continuous and represent patient
    ///    // age. The second variable is dichotomic and give whether
    ///    // they smoke or not (This is completely fictional data).
    ///    double[][] input =
    ///    {
    ///        new double[] { 55, 0 }, // 0 - no cancer
    ///        new double[] { 28, 0 }, // 0
    ///        new double[] { 65, 1 }, // 0
    ///        new double[] { 46, 0 }, // 1 - have cancer
    ///        new double[] { 86, 1 }, // 1
    ///        new double[] { 56, 1 }, // 1
    ///        new double[] { 85, 0 }, // 0
    ///        new double[] { 33, 0 }, // 0
    ///        new double[] { 21, 1 }, // 0
    ///        new double[] { 42, 1 }, // 1
    ///    };
    ///
    ///    // We also know if they have had lung cancer or not, and 
    ///    // we would like to know whether smoking has any connection
    ///    // with lung cancer (This is completely fictional data).
    ///    double[] output =
    ///    {
    ///        0, 0, 0, 1, 1, 1, 0, 0, 0, 1
    ///    };
    ///
    ///
    ///    // To verify this hypothesis, we are going to create a logistic
    ///    // regression model for those two inputs (age and smoking).
    ///    LogisticRegression regression = new LogisticRegression(inputs: 2);
    ///
    ///    // Next, we are going to estimate this model. For this, we
    ///    // will use the Iteratively Reweighted Least Squares method.
    ///    var teacher = new IterativeReweightedLeastSquares(regression);
    ///
    ///    // Now, we will iteratively estimate our model. The Run method returns
    ///    // the maximum relative change in the model parameters and we will use
    ///    // it as the convergence criteria.
    ///
    ///    double delta = 0;
    ///    do
    ///    {
    ///        // Perform an iteration
    ///        delta = teacher.Run(input, output);
    ///
    ///    } while (delta > 0.001);
    ///
    ///    // At this point, we can compute the odds ratio of our variables.
    ///    // In the model, the variable at 0 is always the intercept term, 
    ///    // with the other following in the sequence. Index 1 is the age
    ///    // and index 2 is whether the patient smokes or not.
    ///
    ///    // For the age variable, we have that individuals with
    ///    //   higher age have 1.021 greater odds of getting lung
    ///    //   cancer controlling for cigarette smoking.
    ///    double ageOdds = regression.GetOddsRatio(1); // 1.0208597028836701
    ///
    ///    // For the smoking/non smoking category variable, however, we
    ///    //   have that individuals who smoke have 5.858 greater odds
    ///    //   of developing lung cancer compared to those who do not 
    ///    //   smoke, controlling for age (remember, this is completely
    ///    //   fictional and for demonstration purposes only).
    ///    double smokeOdds = regression.GetOddsRatio(2); // 5.8584748789881331
    ///   </code>
    /// </example>
    /// 
#pragma warning disable 612, 618
    public class IterativeReweightedLeastSquares : IterativeReweightedLeastSquares<GeneralizedLinearRegression>,
        IRegressionFitting
#pragma warning restore 612, 618
    {
        /// <summary>
        ///   Constructs a new Iterative Reweighted Least Squares.
        /// </summary>
        /// 
        /// <param name="regression">The regression to estimate.</param>
        /// 
        public IterativeReweightedLeastSquares(LogisticRegression regression)
        {
            Initialize(GeneralizedLinearRegression.FromLogisticRegression(regression, makeCopy: false));
        }

        /// <summary>
        ///   Constructs a new Iterative Reweighted Least Squares.
        /// </summary>
        /// 
        /// <param name="regression">The regression to estimate.</param>
        /// 
        public IterativeReweightedLeastSquares(GeneralizedLinearRegression regression)
        {
            Initialize(regression);
        }



        /// <summary> 
        /// Runs one iteration of the Reweighted Least Squares algorithm. 
        /// </summary> 
        /// <param name="inputs">The input data.</param> 
        /// <param name="outputs">The outputs associated with each input vector.</param> 
        /// <returns>The maximum relative change in the parameters after the iteration.</returns> 
        ///  
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[] outputs)
        {
            return Run(inputs, outputs.Apply(x => x > 0 ? 1.0 : 0.0));
        }

        /// <summary> 
        /// Runs one iteration of the Reweighted Least Squares algorithm. 
        /// </summary> 
        /// <param name="inputs">The input data.</param> 
        /// <param name="outputs">The outputs associated with each input vector.</param> 
        /// <param name="weights">The weights associated with each sample.</param>
        /// 
        /// <returns>The maximum relative change in the parameters after the iteration.</returns> 
        ///  
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[] outputs, double[] weights)
        {
            return Run(inputs, outputs.Apply(x => x > 0 ? 1.0 : 0.0), weights);
        }

        /// <summary>
        ///   Runs one iteration of the Reweighted Least Squares algorithm.
        /// </summary>
        /// 
        /// <param name="inputs">The input data.</param>
        /// <param name="outputs">The outputs associated with each input vector.</param>
        /// 
        /// <returns>The maximum relative change in the parameters after the iteration.</returns>
        /// 
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[][] outputs)
        {
            if (outputs[0].Length != 1)
                throw new ArgumentException("Function must have a single output.", "outputs");
            double[] output = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
                output[i] = outputs[i][0] > 0 ? 1.0 : 0.0;
            return Run(inputs, output);
        }

        /// <summary>
        ///   Runs one iteration of the Reweighted Least Squares algorithm.
        /// </summary>
        /// 
        /// <param name="inputs">The input data.</param>
        /// <param name="outputs">The outputs associated with each input vector.</param>
        /// <param name="sampleWeight">The weight associated with each sample.</param>
        /// 
        /// <returns>The maximum relative change in the parameters after the iteration.</returns>
        /// 
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, int[][] outputs, double[] sampleWeight)
        {
            if (outputs[0].Length != 1)
                throw new ArgumentException("Function must have a single output.", "outputs");
            double[] output = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
                output[i] = outputs[i][0] > 0 ? 1.0 : 0.0;
            return Run(inputs, output, sampleWeight);
        }

        /// <summary>
        ///   Runs one iteration of the Reweighted Least Squares algorithm.
        /// </summary>
        /// 
        /// <param name="inputs">The input data.</param>
        /// <param name="outputs">The outputs associated with each input vector.</param>
        /// 
        /// <returns>The maximum relative change in the parameters after the iteration.</returns>
        /// 
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, double[][] outputs)
        {
            if (outputs[0].Length != 1)
                throw new ArgumentException("Function must have a single output.", "outputs");

            double[] output = new double[outputs.Length];
            for (int i = 0; i < outputs.Length; i++)
                output[i] = outputs[i][0];

            return Run(inputs, output);
        }

        /// <summary>
        ///   Runs one iteration of the Reweighted Least Squares algorithm.
        /// </summary>
        /// <param name="inputs">The input data.</param>
        /// <param name="outputs">The outputs associated with each input vector.</param>
        /// <returns>The maximum relative change in the parameters after the iteration.</returns>
        /// 
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, double[] outputs)
        {
            return Run(inputs, outputs, null);
        }

        /// <summary>
        ///   Runs one iteration of the Reweighted Least Squares algorithm.
        /// </summary>
        /// 
        /// <param name="inputs">The input data.</param>
        /// <param name="outputs">The outputs associated with each input vector.</param>
        /// <param name="sampleWeights">An weight associated with each sample.</param>
        /// 
        /// <returns>The maximum relative change in the parameters after the iteration.</returns>
        /// 
        [Obsolete("Please use Learn(x, y) instead.")]
        public double Run(double[][] inputs, double[] outputs, double[] sampleWeights)
        {
            int old = Iterations;
            Iterations = 1;
            Learn(inputs, outputs, sampleWeights);
            Iterations = old;
            return Updates.Max();
        }

        /// <summary>
        ///   Computes the sum-of-squared error between the
        ///   model outputs and the expected outputs.
        /// </summary>
        /// 
        /// <param name="inputs">The input data set.</param>
        /// <param name="outputs">The output values.</param>
        /// 
        /// <returns>The sum-of-squared errors.</returns>
        /// 
        [Obsolete("Please use the LogLikelihoodLoss class instead.")]
        public double ComputeError(double[][] inputs, double[] outputs)
        {
            double sum = 0;

            for (int i = 0; i < inputs.Length; i++)
            {
                double actual = Model.Score(inputs[i]);
                double expected = outputs[i];
                double delta = actual - expected;
                sum += delta * delta;
            }

            return sum;
        }

    }

    /// <summary>
    ///   Iterative Reweighted Least Squares for fitting Generalized Linear Models.
    /// </summary>
    /// 
    public class IterativeReweightedLeastSquares<TModel> :
        ISupervisedLearning<TModel, double[], double>,
        ISupervisedLearning<TModel, double[], int>,
        ISupervisedLearning<TModel, double[], bool>,
        IConvergenceLearning
        where TModel : GeneralizedLinearRegression, new()
    {

        private TModel regression;

        private int parameterCount;

        private double[,] hessian;
        private double[] gradient;
        private double[] previous;

        private double lambda = 1e-10;


        private bool computeStandardErrors = true;
        private ISolverMatrixDecomposition<double> decomposition;
        private RelativeParameterConvergence convergence;

        /// <summary>
        ///   Initializes
        /// </summary>
        /// 
        protected void Initialize(TModel regression)
        {
            if (regression == null)
                throw new ArgumentNullException("regression");

            this.regression = regression;
            this.parameterCount = regression.Coefficients.Length;
            this.hessian = new double[parameterCount, parameterCount];
            this.gradient = new double[parameterCount];
        }

        /// <summary>
        ///   Gets or sets the regression model being learned.
        /// </summary>
        /// 
        public TModel Model
        {
            get { return regression; }
            set { regression = value; }
        }

        /// <summary>
        ///   Gets the previous values for the coefficients which were
        ///   in place before the last learning iteration was performed.
        /// </summary>
        /// 
        public double[] Previous { get { return previous; } }

        /// <summary>
        ///   Gets the last parameter updates in the last iteration.
        /// </summary>
        /// 
        public double[] Updates { get { return convergence.NewValues; }} 

        /// <summary>
        ///   Gets the current values for the coefficients.
        /// </summary>
        /// 
        public double[] Solution { get { return regression.Coefficients; } }

        /// <summary>
        ///   Gets the Hessian matrix computed in 
        ///   the last Newton-Raphson iteration.
        /// </summary>
        /// 
        public double[,] Hessian { get { return hessian; } }

        /// <summary>
        ///   Gets the Gradient vector computed in
        ///   the last Newton-Raphson iteration.
        /// </summary>
        /// 
        public double[] Gradient { get { return gradient; } }

        /// <summary>
        ///   Gets the total number of parameters in the model.
        /// </summary>
        /// 
        public int Parameters { get { return parameterCount; } }

        /// <summary>
        /// Gets or sets a cancellation token that can be used to
        /// stop the learning algorithm while it is running.
        /// </summary>
        public CancellationToken Token { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations
        /// performed by the learning algorithm.
        /// </summary>
        public int Iterations
        {
            get { return convergence.Iterations; }
            set { convergence.Iterations = value; }
        }

        /// <summary>
        /// Gets or sets the tolerance value used to determine
        /// whether the algorithm has converged.
        /// </summary>
        public double Tolerance
        {
            get { return convergence.Tolerance; }
            set { convergence.Tolerance = value; }
        }

        /// <summary>
        ///   Gets or sets a value indicating whether standard
        ///   errors should be computed in the next iteration.
        /// </summary>
        /// <value>
        /// 	<c>true</c> to compute standard errors; otherwise, <c>false</c>.
        /// </value>
        /// 
        public bool ComputeStandardErrors
        {
            get { return computeStandardErrors; }
            set { computeStandardErrors = value; }
        }

        /// <summary>
        ///   Gets or sets the regularization value to be
        ///   added in the objective function. Default is
        ///   1e-10.
        /// </summary>
        /// 
        public double Regularization
        {
            get { return lambda; }
            set { lambda = value; }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="IterativeReweightedLeastSquares{TModel}"/> class.
        /// </summary>
        public IterativeReweightedLeastSquares()
        {
            this.convergence = new RelativeParameterConvergence()
            {
                Iterations = 0,
                Tolerance = 1e-5
            };
        }



        /// <summary>
        /// Learns a model that can map the given inputs to the given outputs.
        /// </summary>
        /// <param name="x">The model inputs.</param>
        /// <param name="y">The desired outputs associated with each <paramref name="x">inputs</paramref>.</param>
        /// <param name="weights">The weight of importance for each input-output pair.</param>
        /// <returns>
        /// A model that has learned how to produce <paramref name="y" /> given <paramref name="x" />.
        /// </returns>
        public TModel Learn(double[][] x, int[] y, double[] weights = null)
        {
            return Learn(x, y.ToDouble(), weights);
        }

        /// <summary>
        /// Learns a model that can map the given inputs to the given outputs.
        /// </summary>
        /// <param name="x">The model inputs.</param>
        /// <param name="y">The desired outputs associated with each <paramref name="x">inputs</paramref>.</param>
        /// <param name="weights">The weight of importance for each input-output pair.</param>
        /// <returns>
        /// A model that has learned how to produce <paramref name="y" /> given <paramref name="x" />.
        /// </returns>
        public TModel Learn(double[][] x, bool[] y, double[] weights = null)
        {
            return Learn(x, Classes.ToZeroOne(y), weights);
        }

        /// <summary>
        /// Learns a model that can map the given inputs to the given outputs.
        /// </summary>
        /// <param name="x">The model inputs.</param>
        /// <param name="y">The desired outputs associated with each <paramref name="x">inputs</paramref>.</param>
        /// <param name="weights">The weight of importance for each input-output pair.</param>
        /// <returns>
        /// A model that has learned how to produce <paramref name="y" /> given <paramref name="x" />.
        /// </returns>
        /// <exception cref="DimensionMismatchException">outputs;The number of input vectors and their associated output values must have the same size.</exception>
        public TModel Learn(double[][] x, double[] y, double[] weights = null)
        {
            // Regress using Iteratively Reweighted Least Squares estimation.

            // References:
            //  - Bishop, Christopher M.; Pattern Recognition 
            //    and Machine Learning. Springer; 1st ed. 2006.

            if (x.Length != y.Length)
            {
                throw new DimensionMismatchException("outputs",
                    "The number of input vectors and their associated output values must have the same size.");
            }

            if (regression == null)
            {
                Initialize(new TModel()
                {
                    NumberOfInputs = x.Columns()
                });
            }

            // Initial definitions and memory allocations
            int N = x.Length;

            double[] errors = new double[N];
            double[] w = new double[N];
            double[] coefficients = regression.Coefficients;
            convergence.Clear();

            double[][] design = x.InsertColumn(value: 1, index: 0);

            do
            {
                // Compute errors and weighting matrix
                for (int i = 0; i < x.Length; i++)
                {
                    double actual = regression.Score(x[i]);

                    // Calculate error vector
                    errors[i] = actual - y[i];

                    // Calculate weighting matrix
                    w[i] = regression.Link.Derivative2(actual);
                }

                if (weights != null)
                {
                    for (int i = 0; i < weights.Length; i++)
                    {
                        errors[i] *= weights[i];
                        w[i] *= weights[i];
                    }
                }

                // Reset Hessian matrix and gradient
                Array.Clear(gradient, 0, gradient.Length);
                Array.Clear(hessian, 0, hessian.Length);

                // (Re-) Compute error gradient
                for (int j = 0; j < design.Length; j++)
                    for (int i = 0; i < gradient.Length; i++)
                        gradient[i] += design[j][i] * errors[j] + lambda * coefficients[i];

                // (Re-) Compute weighted "Hessian" matrix 
                for (int k = 0; k < w.Length; k++)
                {
                    double[] row = design[k];
                    for (int j = 0; j < row.Length; j++)
                        for (int i = 0; i < row.Length; i++)
                            hessian[j, i] += row[i] * row[j] * w[k];
                }


                decomposition = new SingularValueDecomposition(hessian);

                double[] deltas = decomposition.Solve(gradient);

                previous = (double[])coefficients.Clone();

                // Update coefficients using the calculated deltas
                for (int i = 0; i < coefficients.Length; i++)
                    coefficients[i] -= deltas[i];

                convergence.NewValues = deltas;

                if (Token.IsCancellationRequested)
                    break;

            } while (!convergence.HasConverged);

            if (computeStandardErrors)
            {
                // Grab the regression information matrix
                double[,] inverse = decomposition.Inverse();

                // Calculate coefficients' standard errors
                double[] standardErrors = regression.StandardErrors;
                for (int i = 0; i < standardErrors.Length; i++)
                    standardErrors[i] = Math.Sqrt(inverse[i, i]);
            }

            return regression;
        }
    }
}
