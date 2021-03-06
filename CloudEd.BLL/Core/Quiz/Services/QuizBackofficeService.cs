﻿using CloudEd.DAL.Repositories;
using CloudEd.DAL.Persistence;
using System;
using CloudEd.BLL.Core.Quiz.Models;
using System.Collections.Generic;
using System.Linq;
using CloudEd.BLL.Core.Question.Models;

namespace CloudEd.BLL.Core.Quiz.Services
{
    public class QuizBackofficeService : IQuizBackofficeService
    {
        private readonly IRepository<DAL.Persistence.Quiz, Guid> _quizRepository;
        private readonly IRepository<DAL.Persistence.Question, Guid> _questionRepository;

        public QuizBackofficeService(
            IRepository<CloudEd.DAL.Persistence.Quiz, Guid> quizRepository,
            IRepository<CloudEd.DAL.Persistence.Question, Guid> questionRepository

            )
        {
            _quizRepository = quizRepository;
            _questionRepository = questionRepository;
        }

        public IEnumerable<QuizEditModel> GetAll()
        {
            return _quizRepository.GetAll().Select(MapQuizToEditModel);
        }

        public void Save(QuizEditModel quiz)
        {
            // TODO : refactor this mess p_0
            var persistenceEntity =_quizRepository.Get(quiz.Id);
            persistenceEntity.Title = quiz.Title;
            persistenceEntity.Description = quiz.Description;
            _quizRepository.Save(persistenceEntity);
        }

        public void Create(QuizCreateModel createModel)
        {
            _quizRepository.Add(MapQuizCreateModelToPersistence(createModel));
        }
        

        private DAL.Persistence.Quiz MapQuizCreateModelToPersistence(QuizCreateModel createModel)
        {
            return new DAL.Persistence.Quiz()
            {
                Id = Guid.NewGuid(),
                Title = createModel.Title,
                Description = createModel.Description,
                QuestionIds = Enumerable.Empty<Guid>()
            };
        }

        private QuizEditModel MapQuizToEditModel(DAL.Persistence.Quiz quiz)
        {
            return new QuizEditModel()
            {
                Id = quiz.Id,
                Title = quiz.Title,
                Description = quiz.Description,
                Questions = GetQuestionsByIds(quiz.QuestionIds)
            };
        }

        private IEnumerable<QuestionEditModel> GetQuestionsByIds(IEnumerable<Guid> questionIds)
        {
            return _questionRepository.GetAll()
                .Join(
                questionIds,
                persistence => persistence.Id,
                id => id,
                (persistence, id) => MapQuestionPersistnenceToEditModel(persistence));
        }

        /// TODO
        public static QuestionEditModel MapQuestionPersistnenceToEditModel(DAL.Persistence.Question question)
        {
            Guid correctAnswerId = question.CorrectAnswer.Id;
            return new QuestionEditModel()
            {
                Id = question.Id,
                Title = question.Title,
                Answers = question.Answers.Select(MapAnswerPersistenceToEditModel)
            };

            AnswerEditModel MapAnswerPersistenceToEditModel(DAL.Persistence.Question.Answer answer)
            {
                return new AnswerEditModel()
                {
                    Id = answer.Id,
                    Body = answer.Body,
                    IsCorrect = IsCorrectAnswer(answer)
                };

                bool IsCorrectAnswer(DAL.Persistence.Question.Answer answ) => answ.Id == correctAnswerId;
                
            }
        }

        public void RemoveQuestion(Guid quizId, Guid questionId)
        {
            ModifyQuizQuestionCollection(quizId,
                (col) => col.Where(id => id != questionId));
        }

        public void AddQuestion(Guid quizId, Guid questionId)
        {
            var collectionAdder = Calculus.Carry<Guid, IEnumerable<Guid>, IEnumerable<Guid>>
                (Calculus.AddIdToCollection, questionId);

            ModifyQuizQuestionCollection(quizId, collectionAdder);
        }


        private void ModifyQuizQuestionCollection(Guid quizId, Func<IEnumerable<Guid>, IEnumerable<Guid>> modifier)
        {
            var quiz = _quizRepository.Get(quizId);
            quiz.QuestionIds = modifier(quiz.QuestionIds);
            _quizRepository.Save(quiz);
        }

        private static class Calculus
        {
            public static Func<TParam2, TResult> Carry<TParam1, TParam2, TResult>
                (Func<TParam1, TParam2, TResult> func, TParam1 param1) 
                    => (TParam2 param2) => func(param1, param2);

            public static IEnumerable<Guid> AddIdToCollection(Guid id, IEnumerable<Guid> collection)
                => collection.Append(id);         
        }

        #region Bulk for now

        public void SaveRelations(Guid quizId, IEnumerable<Guid> questionIds)
        {
            var quiz = _quizRepository.Get(quizId);
            quiz.QuestionIds = questionIds;
            _quizRepository.Save(quiz);
        }

        public void AddRelations(Guid quizId, IEnumerable<Guid> newQuestionIds)
        {
            var quiz = _quizRepository.Get(quizId);
            quiz.QuestionIds = quiz.QuestionIds.Concat(newQuestionIds);
            _quizRepository.Save(quiz);
        }


        public void RemoveRelations(Guid quizId, IEnumerable<Guid> questionIds)
        {
            var quiz = _quizRepository.Get(quizId);
            quiz.QuestionIds = quiz.QuestionIds.Except(questionIds);
            _quizRepository.Save(quiz);
        }
        #endregion
    }
}
