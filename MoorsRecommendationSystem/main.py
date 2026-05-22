from fastapi import FastAPI
from pydantic import BaseModel
import pandas as pd
from sklearn.feature_extraction.text import CountVectorizer
from sklearn.metrics.pairwise import cosine_similarity

app = FastAPI()

movies_df = pd.read_csv("movies.csv")
ratings_df = pd.read_csv("ratings.csv")

movies_df["genres"] = movies_df["genres"].fillna("")

vectorizer = CountVectorizer(tokenizer=lambda x: x.split("|"), token_pattern=None)
genre_matrix = vectorizer.fit_transform(movies_df["genres"])

movie_index_map = pd.Series(movies_df.index, index=movies_df["movieId"]).to_dict()


class RecommendationRequest(BaseModel):
    user_id: int
    top_n: int = 10


@app.get("/")
def root():
    return {"message": "Recommendation service is running."}


@app.post("/recommend")
def recommend_movies(request: RecommendationRequest):
    user_ratings = ratings_df[ratings_df["userId"] == request.user_id]

    if user_ratings.empty:
        return {"userId": request.user_id, "recommendations": []}

    highly_rated = user_ratings[user_ratings["rating"] >= 4.0].head(5)

    if highly_rated.empty:
        return {"userId": request.user_id, "recommendations": []}

    watched_movie_ids = set(user_ratings["movieId"].tolist())
    similarity_scores = {}
    #why the recommended movies were chosen
    reasons = {}

    for _, row in highly_rated.iterrows():
        movie_id = row["movieId"]

        if movie_id not in movie_index_map:
            continue

        idx = movie_index_map[movie_id]
        sim_scores = cosine_similarity(genre_matrix[idx], genre_matrix).flatten()

        for sim_idx, score in enumerate(sim_scores):
            recommended_movie_id = movies_df.iloc[sim_idx]["movieId"]

            if recommended_movie_id in watched_movie_ids:
                continue

            if recommended_movie_id not in similarity_scores:
                similarity_scores[recommended_movie_id] = 0
                reasons[recommended_movie_id] = movie_id

            #weigth by rating
            weight = row["rating"]
            similarity_scores[recommended_movie_id] += score * weight

    sorted_recommendations = sorted(
        similarity_scores.items(),
        key=lambda x: x[1],
        reverse=True
    )[:request.top_n]

    recommended_movies = []
    for movie_id, score in sorted_recommendations:
        movie_row = movies_df[movies_df["movieId"] == movie_id].iloc[0]

        recommended_movies.append({
            "movieId": int(movie_row["movieId"]),
            "title": movie_row["title"],
            "genres": movie_row["genres"],
            "score": float(score),
            "becauseYouLiked": int(reasons[movie_id]) 
        })

    return {
        "userId": request.user_id,
        "recommendations": recommended_movies
    }